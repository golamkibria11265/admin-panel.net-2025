using Bll.Services.FileStorageService.Context.Base;
using Bll.Services.FileStorageService.Files.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bll.Services.FileStorageService.Context.Concrete.AwsS3Bucket
{
    public class AwsS3BucketFileUploadContext : FileContext
    {
        public IFile? File { get; set; }
        public string? BucketName { get; set; }
        public string? ObjectKey { get; set; }
        public string? Identifier { get; set; }
    }
}
