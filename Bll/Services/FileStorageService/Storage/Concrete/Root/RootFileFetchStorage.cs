using Bll.Services.FileStorageService.Context.Concrete.Root;
using Bll.Services.FileStorageService.Storage.Base;
using Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bll.Services.FileStorageService.Storage.Concrete.Root
{
    public class RootFileFetchStorage : IFileFetchStorage<RootFileFetchContext>
    {
        public Task<CommonResponse> FetchAsync(RootFileFetchContext context)
        {
            throw new NotImplementedException();
        }
    }
}
