using System.ComponentModel.DataAnnotations;

namespace Dll.Models.ViewModels
{
    public class LogInViewModel
    {
        [Display(Name = "Username")]
        [Required(ErrorMessage = "User name is required.")]
        [StringLength(256)]
        public string? userName { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(128)]
        public string? password { get; set; }
    }
}
