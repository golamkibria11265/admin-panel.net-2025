using Bll.Services.FileStorageService.Context.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bll.Services.FileStorageService.Factories.Base
{
    public interface IStorageFileFactory<TContext, TStorage> where TContext : FileContext where TStorage : class
    {
        TStorage CreateStorage();
    }
}
