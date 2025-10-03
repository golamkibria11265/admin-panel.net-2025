using Bll.Services.FileStorageService.Context.Concrete.Sftp;
using Bll.Services.FileStorageService.Factories.Base;
using Bll.Services.FileStorageService.Files.Base;
using Bll.Services.FileStorageService.Files.Concrete;
using Bll.Services.FileStorageService.Storage.Base;
using Bll.Services.FileStorageService.Storage.Concrete.Sftp;
using Microsoft.AspNetCore.Http;

namespace Bll.Services.FileStorageService.Factories.Concrete.Sftp
{
    public class SftpImageUploadFactory : IStorageFileUploadFactory<SftpFileUploadContext>
    {
        public IFile CreateFile(IFormFile file)
        {
            return new ImageFile(file);
        }
        public IFileUploadStorage<SftpFileUploadContext> CreateStorage()
        {
            return new SftpFileUploadStorage();
        }
    }
}
