using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class GenericApiResponse<T>
    {
        public int Code { get; set; }
        public List<string>? Messages { get; set; }
        public T? Data { get; set; }
        public string? Details { get; set; }

        public GenericApiResponse()
        {
            Messages = new List<string>();
        }

        public GenericApiResponse(int code, T data)
        {
            Code = code;
            Data = data;
            Messages = new List<string>();
        }

        public GenericApiResponse(int code, string message)
        {
            Code = code;
            Messages = new List<string> { message };
        }
    }
}
