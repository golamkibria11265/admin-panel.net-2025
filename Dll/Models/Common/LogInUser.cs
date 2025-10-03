using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Dll.Models.Common
{
    public class LogInUser
    {
        public ClaimsPrincipal? User { get; set; }
    }
}
