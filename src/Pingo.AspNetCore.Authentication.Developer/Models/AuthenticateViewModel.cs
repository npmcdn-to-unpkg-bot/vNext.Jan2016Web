﻿namespace Pingo.AspNetCore.Authentication.Developer.Models
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