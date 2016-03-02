// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Http.Authentication;

namespace Microsoft.AspNet.Authentication.DeveloperAuth
{
    /// <summary>
    /// Specifies callback methods which the <see cref="DeveloperAuthMiddleware"></see> invokes to enable developer control over the authentication process. />
    /// </summary>
    public interface IDeveloperAuthEvents : IRemoteAuthenticationEvents
    {
        /// <summary>
        /// Invoked whenever DeveloperAuth succesfully authenticates a user
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        Task CreatingTicket(DeveloperAuthCreatingTicketContext context);

        /// <summary>
        /// Called when a Challenge causes a redirect to authorize endpoint in the DeveloperAuth middleware
        /// </summary>
        /// <param name="context">Contains redirect URI and <see cref="AuthenticationProperties"/> of the challenge </param>
        Task RedirectToAuthorizationEndpoint(DeveloperAuthRedirectToAuthorizationEndpointContext context);
    }
}
