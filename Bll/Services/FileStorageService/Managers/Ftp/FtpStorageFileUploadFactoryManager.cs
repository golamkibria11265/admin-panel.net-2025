using Bll.Services.FileStorageService.Context.Concrete.Ftp;
using Bll.Services.FileStorageService.Factories.Base;
using Microsoft.AspNetCore.Http;
using Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bll.Services.FileStorageService.Managers.Ftp
{
    public class FtpStorageFileUploadFactoryManager<TContext> where TContext : FtpFileUploadContext
    {
        private readonly IStorageFileUploadFactory<TContext> _uploadFactory;

        public FtpStorageFileUploadFactoryManager
        (
            IStorageFileUploadFactory<TContext> uploadFactory
        )
        {
            _uploadFactory = uploadFactory;
        }
        public async Task<CommonResponse> UploadAsync(TContext context)
        {
            CommonResponse response = new CommonResponse();
            var storage = _uploadFactory.CreateStorage();
            response = await storage.UploadAsync(context);
            if (response.code != StatusCodes.Status200OK)
            {
                return response;
            }
            response.code = StatusCodes.Status200OK;
            response.messages = new List<string> { "Upload successful" };
            return response;
        }
    }
}
