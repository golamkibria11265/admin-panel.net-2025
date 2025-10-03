using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Exceptions
{
    public class UserChangePinPasswordChangeException : Exception
    {
        public UserChangePinPasswordChangeException()
        {
        }

        public UserChangePinPasswordChangeException(string message)
            : base(message)
        {
        }

        public UserChangePinPasswordChangeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
