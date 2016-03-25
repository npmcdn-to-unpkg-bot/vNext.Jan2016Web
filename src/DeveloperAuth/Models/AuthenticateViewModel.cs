using Microsoft.AspNet.Authentication.P6Common.Messages;

namespace Microsoft.AspNet.Authentication.DeveloperAuth.Models
{
    public class LoginModel
    {
        public string Email { get; set; }
    }

    public class AuthenticateViewModel
    {
        public RequestToken RequestToken { get; set; }
        public LoginModel LoginModel { get; set; }
    }
}