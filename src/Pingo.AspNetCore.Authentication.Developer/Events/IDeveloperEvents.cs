using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Http.Authentication;

namespace Pingo.AspNetCore.Authentication.Developer
{
    /// <summary>
    /// Specifies callback methods which the <see cref="DeveloperMiddleware"></see> invokes to enable developer control over the authentication process. />
    /// </summary>
    public interface IDeveloperEvents : IRemoteAuthenticationEvents
    {
        /// <summary>
        /// Invoked whenever Developer succesfully authenticates a user
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        Task CreatingTicket(DeveloperCreatingTicketContext context);

        /// <summary>
        /// Called when a Challenge causes a redirect to authorize endpoint in the Developer middleware
        /// </summary>
        /// <param name="context">Contains redirect URI and <see cref="AuthenticationProperties"/> of the challenge </param>
        Task RedirectToAuthorizationEndpoint(DeveloperRedirectToAuthorizationEndpointContext context);
    }
}
