using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class ElasticRequestResponse
    {
        public DateTime RequestTime { get; set; }
        public string? LocalIPAddress { get; set; }
        public string? LocalPort { get; set; }
        public string? RemoteIPAddress { get; set; }
        public string? RemotePort { get; set; }
        public string? UserAgent { get; set; }
        public DateTime ResponseTime { get; set; }
        public string? RequestSchema { get; set; }
        public string? RequestHost { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestQueryString { get; set; }
        public object? RequestBody { get; set; }
        public object? ResponseBody { get; set; }
        public int StatusCode { get; set; }
    }
}
