using Bll.Services.FileStorageService.Context.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bll.Services.FileStorageService.Context.Concrete.Ftp
{
    public class FtpFileFetchContext : FileContext
    {
        public string? Identifier { get; set; }
    }
}
