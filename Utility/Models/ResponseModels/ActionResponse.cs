using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Models.ResponseModels
{
    public class ActionResponse: ApiResponse
    {
        public bool is_valid { get; set; } = false;
        public bool session_expired { get; set; } = false;
        public string? redirect_url { get; set; }
        public string? html { get; set; }
    }
}
