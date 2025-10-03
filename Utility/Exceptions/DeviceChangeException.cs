using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Exceptions
{
    public class DeviceChangeException : Exception
    {
        public DeviceChangeException()
        {
        }

        public DeviceChangeException(string message)
            : base(message)
        {
        }

        public DeviceChangeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
