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
    public class SftpFileUploadStorage : IFileUploadStorage<SftpFileUploadContext>
    {
        public Task<CommonResponse> UploadAsync(SftpFileUploadContext context)
        {
            throw new NotImplementedException();
        }
    }
}
