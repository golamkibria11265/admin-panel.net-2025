using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Exceptions
{
    public class CustomApplicationException : Exception
    {
        public CustomApplicationException()
        {

        }
        public CustomApplicationException(string message) : base(message)
        {

        }
        public CustomApplicationException(string message, int v) : base(message)
        {

        }
        public CustomApplicationException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
