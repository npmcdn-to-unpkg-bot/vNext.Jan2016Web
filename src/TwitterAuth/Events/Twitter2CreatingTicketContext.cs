// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;

namespace Microsoft.AspNet.Authentication.Twitter2
{
    /// <summary>
    /// Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.
    /// </summary>
    public class Twitter2CreatingTicketContext : BaseTwitter2Context
    {
        /// <summary>
        /// Initializes a <see cref="Twitter2CreatingTicketContext"/>
        /// </summary>
        /// <param name="context">The HTTP environment</param>
        /// <param name="userId">Twitter2 user ID</param>
        /// <param name="screenName">Twitter2 screen name</param>
        /// <param name="accessToken">Twitter2 access token</param>
        /// <param name="accessTokenSecret">Twitter2 access token secret</param>
        public Twitter2CreatingTicketContext(
            HttpContext context,
            Twitter2Options options,
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
        /// Gets the Twitter2 user ID
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Gets the Twitter2 screen name
        /// </summary>
        public string ScreenName { get; private set; }

        /// <summary>
        /// Gets the Twitter2 access token
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// Gets the Twitter2 access token secret
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
