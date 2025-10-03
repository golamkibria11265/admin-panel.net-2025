using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Models.RequestModels;

namespace Dll.Models.RequestModels
{
    public class MenuParamsRequest: PaginationParamsRequest
    {
        public string? SearchString { get; set; }
        public string? Parent { get; set; }
    }
}
