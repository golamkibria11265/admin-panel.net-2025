using Bll.Services.FileStorageService.Context.Base;
using Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bll.Services.FileStorageService.Storage.Base
{
    public interface IFileExistStorage<TContext> where TContext : FileContext
    {
        Task<CommonResponse> ExistAsync(TContext context);
    }
}
