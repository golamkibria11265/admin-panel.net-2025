using Bll.Services.FileStorageService.Context.Concrete.AwsS3Bucket;
using Bll.Services.FileStorageService.Storage.Base;
using Utility;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bll.Services.FileStorageService.Storage.Concrete.AwsS3Bucket
{
    public class AwsS3BucketFileUploadStorage : IFileUploadStorage<AwsS3BucketFileUploadContext>
    {
        public Task<CommonResponse> UploadAsync(AwsS3BucketFileUploadContext context)
        {
            throw new NotImplementedException();
        }
    }
}
