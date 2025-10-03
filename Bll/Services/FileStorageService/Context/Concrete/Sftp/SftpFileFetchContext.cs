using Bll.Services.FileStorageService.Context.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bll.Services.FileStorageService.Context.Concrete.Sftp
{
    public class SftpFileFetchContext : FileContext
    {
        public string? Identifier { get; set; }
    }
}
