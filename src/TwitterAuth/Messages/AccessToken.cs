// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Authentication.Twitter2
{
    /// <summary>
    /// The Twitter2 access token retrieved from the access token endpoint.
    /// </summary>
    public class AccessToken : RequestToken
    {
        /// <summary>
        /// Gets or sets the Twitter2 User ID.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the Twitter2 screen name.
        /// </summary>
        public string ScreenName { get; set; }
    }
}
