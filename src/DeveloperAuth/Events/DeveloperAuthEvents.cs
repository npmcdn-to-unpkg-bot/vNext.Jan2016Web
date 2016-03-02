// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Authentication;

namespace Microsoft.AspNet.Authentication.DeveloperAuth
{
    /// <summary>
    /// Default <see cref="IDeveloperAuthEvents"/> implementation.
    /// </summary>
    public class DeveloperAuthEvents : RemoteAuthenticationEvents, IDeveloperAuthEvents
    {
        /// <summary>
        /// Gets or sets the function that is invoked when the Authenticated method is invoked.
        /// </summary>
        public Func<DeveloperAuthCreatingTicketContext, Task> OnCreatingTicket { get; set; } = context => Task.FromResult(0);

        /// <summary>
        /// Gets or sets the delegate that is invoked when the ApplyRedirect method is invoked.
        /// </summary>
        public Func<DeveloperAuthRedirectToAuthorizationEndpointContext, Task> OnRedirectToAuthorizationEndpoint { get; set; } = context =>
        {
            context.Response.Redirect(context.RedirectUri);
            return Task.FromResult(0);
        };

        /// <summary>
        /// Invoked whenever DeveloperAuth successfully authenticates a user
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        public virtual Task CreatingTicket(DeveloperAuthCreatingTicketContext context) => OnCreatingTicket(context);

        /// <summary>
        /// Called when a Challenge causes a redirect to authorize endpoint in the DeveloperAuth middleware
        /// </summary>
        /// <param name="context">Contains redirect URI and <see cref="AuthenticationProperties"/> of the challenge </param>
        public virtual Task RedirectToAuthorizationEndpoint(DeveloperAuthRedirectToAuthorizationEndpointContext context) => OnRedirectToAuthorizationEndpoint(context);
    }
}
