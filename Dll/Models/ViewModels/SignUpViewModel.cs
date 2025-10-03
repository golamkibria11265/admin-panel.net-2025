using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dll.Models.ViewModels
{
    public class SignUpViewModel
    {
        [Display(Name = "Username")]
        public string? UserName { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Display(Name = "Mobile")]
        public string? Mobile { get; set; }
    }
}
