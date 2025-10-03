using Bll.Services.FileStorageService.Context.Concrete.Root;
using Bll.Services.FileStorageService.Factories.Base;
using Bll.Services.FileStorageService.Files.Base;
using Bll.Services.FileStorageService.Files.Concrete;
using Bll.Services.FileStorageService.Storage.Base;
using Bll.Services.FileStorageService.Storage.Concrete.Root;
using Microsoft.AspNetCore.Http;

namespace Bll.Services.FileStorageService.Factories.Concrete.Root
{
    public class RootImageUploadFactory : IStorageFileUploadFactory<RootFileUploadContext>
    {
        public IFile CreateFile(IFormFile file)
        {
            return new ImageFile(file);
        }
        public IFileUploadStorage<RootFileUploadContext> CreateStorage()
        {
            return new RootFileUploadStorage();
        }
    }
}
