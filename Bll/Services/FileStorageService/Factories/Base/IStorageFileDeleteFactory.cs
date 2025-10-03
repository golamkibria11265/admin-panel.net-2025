using Bll.Services.FileStorageService.Context.Base;
using Bll.Services.FileStorageService.Storage.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bll.Services.FileStorageService.Factories.Base
{
    public interface IFileDeleteStorageFactory<TContext> : IStorageFileFactory<TContext, IFileDeleteStorage<TContext>> where TContext : FileContext
    {
    }
}
