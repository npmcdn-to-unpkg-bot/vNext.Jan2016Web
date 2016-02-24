using System.ComponentModel.DataAnnotations;

namespace Pingo.Authorization.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
