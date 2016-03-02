// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Authentication.DeveloperAuth;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// Extension methods to add DeveloperAuth authentication capabilities to an HTTP application pipeline.
    /// </summary>
    public static class DeveloperAuthAppBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="DeveloperAuthMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables DeveloperAuth authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="configureOptions">An action delegate to configure the provided <see cref="DeveloperAuthOptions"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseDeveloperAuthAuthentication(this IApplicationBuilder app, Action<DeveloperAuthOptions> configureOptions = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = new DeveloperAuthOptions();
            if (configureOptions != null)
            {
                configureOptions(options);
            }
            return app.UseDeveloperAuthAuthentication(options);
        }

        /// <summary>
        /// Adds the <see cref="DeveloperAuthMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables DeveloperAuth authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="options">A <see cref="DeveloperAuthOptions"/> that specifies options for the middleware.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseDeveloperAuthAuthentication(this IApplicationBuilder app, DeveloperAuthOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<DeveloperAuthMiddleware>(options);
        }
    }
}
