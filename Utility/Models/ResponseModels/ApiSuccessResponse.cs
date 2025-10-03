using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class ApiSuccessResponse
    {
        public int code { get; set; }
        public List<string>? messages { get; set; }
        public object? data { get; set; }
        public string? details { get; set; }
    }
}
