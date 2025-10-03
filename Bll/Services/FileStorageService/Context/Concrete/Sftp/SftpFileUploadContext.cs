using Bll.Services.FileStorageService.Context.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bll.Services.FileStorageService.Context.Concrete.Sftp
{
    public class SftpFileUploadContext : FileContext
    {
        public string? SourcePath { get; set; }
        public string? DestinationPath { get; set; }
        public string? Identifier { get; set; }
    }
}
