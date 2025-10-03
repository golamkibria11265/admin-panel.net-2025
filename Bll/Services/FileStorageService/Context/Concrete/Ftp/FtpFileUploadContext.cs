using Bll.Services.FileStorageService.Context.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bll.Services.FileStorageService.Context.Concrete.Ftp
{
    public class FtpFileUploadContext : FileContext
    {
        public string? SourcePath { get; set; }
        public string? DestinationPath { get; set; }
        public string? Identifier { get; set; }
    }
}
