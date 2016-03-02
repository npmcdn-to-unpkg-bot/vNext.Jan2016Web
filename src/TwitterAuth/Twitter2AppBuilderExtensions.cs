// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authentication.Twitter2;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// Extension methods to add Twitter2 authentication capabilities to an HTTP application pipeline.
    /// </summary>
    public static class Twitter2AppBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="Twitter2Middleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables Twitter2 authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="configureOptions">An action delegate to configure the provided <see cref="Twitter2Options"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseTwitter2Authentication(this IApplicationBuilder app, Action<Twitter2Options> configureOptions = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = new Twitter2Options();
            if (configureOptions != null)
            {
                configureOptions(options);
            }
            return app.UseTwitter2Authentication(options);
        }

        /// <summary>
        /// Adds the <see cref="Twitter2Middleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables Twitter2 authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="options">A <see cref="Twitter2Options"/> that specifies options for the middleware.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseTwitter2Authentication(this IApplicationBuilder app, Twitter2Options options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<Twitter2Middleware>(options);
        }
    }
}
