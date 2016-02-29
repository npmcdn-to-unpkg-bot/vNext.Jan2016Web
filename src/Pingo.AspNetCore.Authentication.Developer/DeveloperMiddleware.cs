using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;

namespace Pingo.AspNetCore.Authentication.Developer
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "Middleware are not disposable.")]
    public class DeveloperMiddleware : AuthenticationMiddleware<DeveloperOptions>
    {
        private readonly HttpClient _httpClient;

        public DeveloperMiddleware(RequestDelegate next,
             IDataProtectionProvider dataProtectionProvider,
            DeveloperOptions options, ILoggerFactory loggerFactory,
            IUrlEncoder encoder)
            : base(next, options, loggerFactory, encoder)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

          

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            if (DeveloperOptions.StateDataFormat == null)
            {
                var dataProtector = dataProtectionProvider.CreateProtector(
                    typeof(DeveloperMiddleware).FullName, Options.AuthenticationScheme, "v1");
                DeveloperOptions.StateDataFormat = new SecureDataFormat<RequestToken>(
                    new RequestTokenSerializer(),
                    dataProtector);
            }


            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
        }

        protected override AuthenticationHandler<DeveloperOptions> CreateHandler()
        {
            return new DeveloperHandler(_httpClient);
        }
    }
}
