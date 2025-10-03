using Bll.Services.FileStorageService.Context.Concrete.Ftp;
using Bll.Services.FileStorageService.Storage.Concrete.Ftp;
using Bll.Services.FileStorageService.Files.Concrete;
using Bll.Services.FileStorageService.Factories.Base;
using Bll.Services.FileStorageService.Storage.Base;
using Bll.Services.FileStorageService.Files.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace Bll.Services.FileStorageService.Factories.Concrete.Ftp
{
    public class FtpImageUploadFactory : IStorageFileUploadFactory<FtpFileUploadContext>
    {
        private readonly IConfiguration _iConfiguration;
        private readonly Serilog.ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public FtpImageUploadFactory
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
        public IFile CreateFile(IFormFile file)
        {
            return new ImageFile(file);
        }
        public IFileUploadStorage<FtpFileUploadContext> CreateStorage()
        {
            return new FtpFileUploadStorage(_iConfiguration, _logger, _httpClientFactory);
        }
    }
}
