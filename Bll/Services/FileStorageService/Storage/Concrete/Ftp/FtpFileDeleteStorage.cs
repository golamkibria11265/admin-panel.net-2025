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
    public class FtpFileDeleteStorage : IFileDeleteStorage<FtpFileDeleteContext>
    {
        public Task<CommonResponse> DeleteAsync(FtpFileDeleteContext file)
        {
            throw new NotImplementedException();
        }
    }
}
