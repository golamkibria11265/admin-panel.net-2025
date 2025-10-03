using Bll.Services.FileStorageService.Context.Concrete.AwsS3Bucket;
using Bll.Services.FileStorageService.Factories.Base;
using Bll.Services.FileStorageService.Storage.Base;
using Bll.Services.FileStorageService.Storage.Concrete.AwsS3Bucket;

namespace Bll.Services.FileStorageService.Factories.Concrete.AwsS3Bucket
{
    public class AwsS3BucketImageDeleteFactory : IFileDeleteStorageFactory<AwsS3BucketFileDeleteContext>
    {
        public IFileDeleteStorage<AwsS3BucketFileDeleteContext> CreateStorage()
        {
            return new AwsS3BucketFileDeleteStorage();
        }
    }
}
