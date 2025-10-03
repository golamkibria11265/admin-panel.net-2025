using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Exceptions
{
    public class CustomTransactionException: Exception
    {
        public int Code { get; set; }
        public CustomTransactionException(string message, int code):base(message)
        {
            Code = code;
        }
    }
}
