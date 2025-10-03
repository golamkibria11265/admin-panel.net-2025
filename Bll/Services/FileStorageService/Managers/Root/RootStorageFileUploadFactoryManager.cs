using Bll.Services.FileStorageService.Context.Concrete.Root;
using Bll.Services.FileStorageService.Factories.Base;
using Microsoft.AspNetCore.Http;
using Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bll.Services.FileStorageService.Managers.Root
{
    public class RootStorageFileUploadFactoryManager<TContext> where TContext : RootFileUploadContext
    {
        private readonly IStorageFileUploadFactory<TContext> _uploadFactory;

        public RootStorageFileUploadFactoryManager
        (
            IStorageFileUploadFactory<TContext> uploadFactory
        )
        {
            _uploadFactory = uploadFactory;
        }
        public async Task<CommonResponse> UploadAsync(TContext context)
        {
            CommonResponse response = new CommonResponse();

            if (context.File == null)
            {
                response.code = StatusCodes.Status400BadRequest;
                response.messages = new List<string> { "No file provided" };
                return response;
            }
            response = await context.File.ValidateAsync();
            if (response.code != StatusCodes.Status200OK)
            {
                return response;
            }
            var storage = _uploadFactory.CreateStorage();
            response = await storage.UploadAsync(context);
            if (response.code != StatusCodes.Status200OK)
            {
                return response;
            }
            response.code = StatusCodes.Status200OK;
            response.messages = new List<string> { "Upload successful" };
            response.data = new { context.File.Identifier };

            return response;
        }
    }
}
