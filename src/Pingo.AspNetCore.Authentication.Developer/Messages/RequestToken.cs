using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Authentication;

namespace Pingo.AspNetCore.Authentication.Developer
{
    /// <summary>
    /// The Developer request token obtained from the request token endpoint.
    /// </summary>
    public class RequestToken
    {
        /// <summary>
        /// Gets or sets the Developer request token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the Developer token secret.
        /// </summary>
        public string TokenSecret { get; set; }

        /// <summary>
        /// Gets or sets the Developer CallBackUri.
        /// </summary>
        public string CallBackUri { get; set; }

        public bool CallbackConfirmed { get; set; }

        /// <summary>
        /// Gets or sets a property bag for common authentication properties.
        /// </summary>
        public AuthenticationProperties Properties { get; set; }
    }
}
