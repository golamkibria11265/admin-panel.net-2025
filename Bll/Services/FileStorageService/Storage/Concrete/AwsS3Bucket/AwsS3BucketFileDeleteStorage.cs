using Bll.Services.FileStorageService.Context.Concrete.AwsS3Bucket;
using Bll.Services.FileStorageService.Storage.Base;
using Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bll.Services.FileStorageService.Storage.Concrete.AwsS3Bucket
{
    public class AwsS3BucketFileDeleteStorage : IFileDeleteStorage<AwsS3BucketFileDeleteContext>
    {
        public Task<CommonResponse> DeleteAsync(AwsS3BucketFileDeleteContext context)
        {
            throw new NotImplementedException();
        }
    }
}
