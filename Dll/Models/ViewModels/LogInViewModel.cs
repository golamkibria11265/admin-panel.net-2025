using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dll.Models.ViewModels
{
    public class LogInViewModel
    {
        [Display(Name = "Username")]
        public string? userName { get; set; }

        [Display(Name = "Password")]
        public string? password { get; set; }
    }
}
