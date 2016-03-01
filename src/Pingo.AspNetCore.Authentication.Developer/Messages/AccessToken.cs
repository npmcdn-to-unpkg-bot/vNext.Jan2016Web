﻿namespace Pingo.AspNetCore.Authentication.Developer
{
    /// <summary>
    /// The Developer access token retrieved from the access token endpoint.
    /// </summary>
    public class AccessToken : RequestToken
    {
        /// <summary>
        /// Gets or sets the Twitter User ID.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the Twitter screen name.
        /// </summary>
        public string ScreenName { get; set; }
    }
}