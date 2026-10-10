using System.ComponentModel.DataAnnotations;

namespace Dll.Models.ViewModels
{
    public class SignUpViewModel
    {
        [Display(Name = "Username")]
        [Required(ErrorMessage = "User name is required.")]
        [StringLength(256, MinimumLength = 3, ErrorMessage = "User name must be 3 to 256 characters.")]
        [RegularExpression(@"^[A-Za-z0-9._@-]+$", ErrorMessage = "User name may contain only letters, digits and . _ @ -")]
        public string? UserName { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        [StringLength(256)]
        public string? Email { get; set; }

        // PCI-DSS 8.3.6: at least 12 characters with both letters and digits.
        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(128, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain both letters and digits.")]
        public string? Password { get; set; }

        [Display(Name = "Mobile")]
        [Phone(ErrorMessage = "Mobile number is not valid.")]
        [StringLength(20)]
        public string? Mobile { get; set; }
    }
}
