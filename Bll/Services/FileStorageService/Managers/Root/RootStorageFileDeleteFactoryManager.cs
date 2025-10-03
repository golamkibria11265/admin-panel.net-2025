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
    public class RootStorageFileDeleteFactoryManager<TContext> where TContext : RootFileDeleteContext
    {
        private readonly IFileDeleteStorageFactory<TContext> _deleteFactory;

        public RootStorageFileDeleteFactoryManager
        (
            IFileDeleteStorageFactory<TContext> deleteFactory
        )
        {
            _deleteFactory = deleteFactory;
        }
        public async Task<CommonResponse> DeleteAsync(TContext context)
        {
            CommonResponse response = new CommonResponse();
            if (string.IsNullOrEmpty(context.Identifier))
            {
                response.code = StatusCodes.Status400BadRequest;
                response.messages = new List<string> { "No identifier provided" };
                return response;
            }
            var storage = _deleteFactory.CreateStorage();
            response = await storage.DeleteAsync(context);
            if (response.code != StatusCodes.Status200OK)
            {
                return response;
            }
            response.code = StatusCodes.Status200OK;
            response.messages = new List<string> { "Delete successful" };
            return response;
        }
    }
}
