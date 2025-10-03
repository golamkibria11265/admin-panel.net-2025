using Bll.Services.FileStorageService.Context.Concrete.Ftp;
using Bll.Services.FileStorageService.Storage.Base;
using Microsoft.Extensions.Configuration;
using Utility;
using System.Net;
using FluentFTP;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bll.Services.FileStorageService.Storage.Concrete.Ftp
{
    public class FtpFileUploadStorage : IFileUploadStorage<FtpFileUploadContext>
    {
        private readonly IConfiguration _iConfiguration;
        private readonly Serilog.ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public FtpFileUploadStorage
        (
             IConfiguration iConfiguration,
             Serilog.ILogger logger,
             IHttpClientFactory httpClientFactory
        )
        {
            _iConfiguration = iConfiguration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<CommonResponse> UploadAsync(FtpFileUploadContext context)
        {
            CommonResponse commonResponse = new();

            if (string.IsNullOrWhiteSpace(context.Identifier))
            {
                commonResponse.code = StatusCodes.Status422UnprocessableEntity;
                commonResponse.messages = new List<string> { "Identifier is required." };
                return commonResponse;
            }
            if (string.IsNullOrWhiteSpace(context.SourcePath))
            {
                commonResponse.code = StatusCodes.Status422UnprocessableEntity;
                commonResponse.messages = new List<string> { "Source path is required." };
                return commonResponse;
            }
            if (string.IsNullOrWhiteSpace(context.DestinationPath))
            {
                commonResponse.code = StatusCodes.Status422UnprocessableEntity;
                commonResponse.messages = new List<string> { "Destination path is required." };
                return commonResponse;
            }

            var host = _iConfiguration.GetValue<string>("FileStorageConfig:Ftp:Host");
            var userName = _iConfiguration.GetValue<string>("FileStorageConfig:Ftp:UserName");
            var password = _iConfiguration.GetValue<string>("FileStorageConfig:Ftp:Password");

            try
            {
                using var client = new FtpClient(host, new NetworkCredential(userName, password));
                client.Connect();
                if (!client.IsConnected)
                {
                    _logger.Error("FTP connection failed.");
                    _logger.Error($"FTP server reply code: {client.LastReply.Code}");
                    _logger.Error($"FTP server reply message: {client.LastReply.Message}");

                    commonResponse.code = StatusCodes.Status500InternalServerError;
                    commonResponse.messages = new List<string> { "FTP connection failed." };
                    return commonResponse;
                }
                var normalizedDestinationPath = context.DestinationPath.Replace("\\", "/");
                string[] parts = normalizedDestinationPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                string current = string.Empty;
                foreach (var part in parts)
                {
                    current += "/" + part;
                    if (!client.DirectoryExists(current))
                    {
                        client.CreateDirectory(current);
                    }
                }
                string localFile = Path.Combine(context.SourcePath, context.Identifier);
                string remoteFile = Path.Combine(context.DestinationPath, context.Identifier).Replace("\\", "/");

                client.UploadFile(localFile, remoteFile, FtpRemoteExists.Overwrite, true);

                commonResponse.code = StatusCodes.Status200OK;
                commonResponse.messages = new List<string> { "File uploaded successfully to ftp server." };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "FTP upload failed.");

                commonResponse.code = StatusCodes.Status500InternalServerError;
                commonResponse.messages = new List<string> { ex.Message };
            }
            await Task.CompletedTask;
            return commonResponse;
        }
    }
}
