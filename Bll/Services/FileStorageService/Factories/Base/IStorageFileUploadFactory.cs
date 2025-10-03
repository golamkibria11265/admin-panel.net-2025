using Bll.Services.FileStorageService.Context.Base;
using Bll.Services.FileStorageService.Files.Base;
using Bll.Services.FileStorageService.Storage.Base;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bll.Services.FileStorageService.Factories.Base
{
    public interface IStorageFileUploadFactory<TContext> : IStorageFileFactory<TContext, IFileUploadStorage<TContext>> where TContext : FileContext
    {
        IFile CreateFile(IFormFile file);
    }
}
