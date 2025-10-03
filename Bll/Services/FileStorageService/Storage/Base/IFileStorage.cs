using Bll.Services.FileStorageService.Context.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bll.Services.FileStorageService.Storage.Base
{
    public interface IFileStorage<TContext> :
        IFileUploadStorage<TContext>,
        IFileDeleteStorage<TContext>,
        IFileFetchStorage<TContext>,
        IFileExistStorage<TContext>
        where TContext : FileContext
    {
    }
}
