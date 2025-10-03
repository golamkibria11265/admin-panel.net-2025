using Bll.Services.FileStorageService.Context.Concrete.Ftp;
using Bll.Services.FileStorageService.Storage.Base;
using Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bll.Services.FileStorageService.Storage.Concrete.Ftp
{
    public class FtpFileFetchStorage : IFileFetchStorage<FtpFileFetchContext>
    {
        public Task<CommonResponse> FetchAsync(FtpFileFetchContext file)
        {
            throw new NotImplementedException();
        }
    }
}
