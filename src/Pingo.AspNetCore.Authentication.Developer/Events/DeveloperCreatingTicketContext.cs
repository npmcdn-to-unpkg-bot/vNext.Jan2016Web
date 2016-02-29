using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;

namespace Pingo.AspNetCore.Authentication.Developer
{
    /// <summary>
    /// Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.
    /// </summary>
    public class DeveloperCreatingTicketContext : BaseDeveloperContext
    {
        /// <summary>
        /// Initializes a <see cref="DeveloperCreatingTicketContext"/>
        /// </summary>
        /// <param name="context">The HTTP environment</param>
        /// <param name="userId">Developer user ID</param>
        /// <param name="screenName">Developer screen name</param>
        /// <param name="accessToken">Developer access token</param>
        /// <param name="accessTokenSecret">Developer access token secret</param>
        public DeveloperCreatingTicketContext(
            HttpContext context,
            DeveloperOptions options,
            string userId,
            string screenName,
            string accessToken,
            string accessTokenSecret)
            : base(context, options)
        {
            UserId = userId;
            ScreenName = screenName;
            AccessToken = accessToken;
            AccessTokenSecret = accessTokenSecret;
        }

        /// <summary>
        /// Gets the Developer user ID
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Gets the Developer screen name
        /// </summary>
        public string ScreenName { get; private set; }

        /// <summary>
        /// Gets the Developer access token
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// Gets the Developer access token secret
        /// </summary>
        public string AccessTokenSecret { get; private set; }

        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> representing the user
        /// </summary>
        public ClaimsPrincipal Principal { get; set; }

        /// <summary>
        /// Gets or sets a property bag for common authentication properties
        /// </summary>
        public AuthenticationProperties Properties { get; set; }
    }
}
