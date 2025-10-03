using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Models.RequestModels;
using Utility.Models.ResponseModels;

namespace Utility
{
    public class ApiResponse : PaginationParamsResponse
    {
        public int code { get; set; }
        public List<string>? messages { get; set; }
        public object? data { get; set; }
        public string? details { get; set; }
    }
}
