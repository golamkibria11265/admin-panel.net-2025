using Bll.Services.FileStorageService.Context.Concrete.AwsS3Bucket;
using Bll.Services.FileStorageService.Factories.Base;
using Bll.Services.FileStorageService.Storage.Base;
using Bll.Services.FileStorageService.Storage.Concrete.AwsS3Bucket;

namespace Bll.Services.FileStorageService.Factories.Concrete.AwsS3Bucket
{
    public class AwsS3BucketImageFetchFactory : IFileFetchStorageFactory<AwsS3BucketFileFetchContext>
    {
        public IFileFetchStorage<AwsS3BucketFileFetchContext> CreateStorage()
        {
            return new AwsS3BucketFileFetchStorage();
        }
    }
}
