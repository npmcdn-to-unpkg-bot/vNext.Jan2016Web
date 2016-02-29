using System;
using Microsoft.AspNet.Builder;


namespace Pingo.AspNetCore.Authentication.Developer
{
    public static class DeveloperAppBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="DeveloperMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables Developer authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseDeveloperAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<DeveloperMiddleware>();
        }

        /// <summary>
        /// Adds the <see cref="DeveloperMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables Developer authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="options">A <see cref="DeveloperOptions"/> that specifies options for the middleware.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseDeveloperAuthentication(this IApplicationBuilder app, DeveloperOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<DeveloperMiddleware>(options );
        }
    }
}
