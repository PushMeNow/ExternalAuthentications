using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;

namespace ExternalAuthentications.Models
{
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public IEnumerable<AuthenticationProviderModel> ExternalProviders { get; set; }
    }
}