// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Authentication.Twitter2
{
    /// <summary>
    /// Options for the Twitter2 authentication middleware.
    /// </summary>
    public class Twitter2Options : RemoteAuthenticationOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Twitter2Options"/> class.
        /// </summary>
        public Twitter2Options()
        {
            AuthenticationScheme = Twitter2Defaults.AuthenticationScheme;
            DisplayName = AuthenticationScheme;
            CallbackPath = new PathString("/signin-twitter2");
            BackchannelTimeout = TimeSpan.FromSeconds(60);
            Events = new Twitter2Events();
        }

        /// <summary>
        /// Gets or sets the consumer key used to communicate with Twitter2.
        /// </summary>
        /// <value>The consumer key used to communicate with Twitter2.</value>
        public string ConsumerKey { get; set; }

        /// <summary>
        /// Gets or sets the consumer secret used to sign requests to Twitter2.
        /// </summary>
        /// <value>The consumer secret used to sign requests to Twitter2.</value>
        public string ConsumerSecret { get; set; }

        /// <summary>
        /// Gets or sets the type used to secure data handled by the middleware.
        /// </summary>
        public ISecureDataFormat<RequestToken> StateDataFormat { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITwitter2Events"/> used to handle authentication events.
        /// </summary>
        public new ITwitter2Events Events
        {
            get { return (ITwitter2Events)base.Events; }
            set { base.Events = value; }
        }

        /// <summary>
        /// Defines whether access tokens should be stored in the
        /// <see cref="ClaimsPrincipal"/> after a successful authentication.
        /// This property is set to <c>false</c> by default to reduce
        /// the size of the final authentication cookie.
        /// </summary>
        public bool SaveTokensAsClaims { get; set; }
    }
}
