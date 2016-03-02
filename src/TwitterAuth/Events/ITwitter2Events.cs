// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.Authentication.Twitter2
{
    /// <summary>
    /// Specifies callback methods which the <see cref="Twitter2Middleware"></see> invokes to enable developer control over the authentication process. />
    /// </summary>
    public interface ITwitter2Events : IRemoteAuthenticationEvents
    {
        /// <summary>
        /// Invoked whenever Twitter2 succesfully authenticates a user
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        Task CreatingTicket(Twitter2CreatingTicketContext context);

        /// <summary>
        /// Called when a Challenge causes a redirect to authorize endpoint in the Twitter2 middleware
        /// </summary>
        /// <param name="context">Contains redirect URI and <see cref="AuthenticationProperties"/> of the challenge </param>
        Task RedirectToAuthorizationEndpoint(Twitter2RedirectToAuthorizationEndpointContext context);
    }
}
