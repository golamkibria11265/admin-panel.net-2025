using Bll.Services.FileStorageService.Context.Concrete.Sftp;
using Bll.Services.FileStorageService.Storage.Base;
using Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bll.Services.FileStorageService.Storage.Concrete.Sftp
{
    public class SftpFileFetchStorage : IFileFetchStorage<SftpFileFetchContext>
    {
        public Task<CommonResponse> FetchAsync(SftpFileFetchContext context)
        {
            throw new NotImplementedException();
        }
    }
}
