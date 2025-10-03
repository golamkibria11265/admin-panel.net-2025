using Bll.Services.FileStorageService.Context.Concrete.AwsS3Bucket;
using Bll.Services.FileStorageService.Factories.Base;
using Bll.Services.FileStorageService.Files.Base;
using Bll.Services.FileStorageService.Files.Concrete;
using Bll.Services.FileStorageService.Storage.Base;
using Bll.Services.FileStorageService.Storage.Concrete.AwsS3Bucket;
using Microsoft.AspNetCore.Http;

namespace Bll.Services.FileStorageService.Factories.Concrete.AwsS3Bucket
{
    public class AwsS3BucketImageUploadFactory : IStorageFileUploadFactory<AwsS3BucketFileUploadContext>
    {
        public IFile CreateFile(IFormFile file)
        {
            return new ImageFile(file);
        }
        public IFileUploadStorage<AwsS3BucketFileUploadContext> CreateStorage()
        {
            return new AwsS3BucketFileUploadStorage();
        }
    }
}
