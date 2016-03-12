using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Pingo.Core.Providers;
using Pingo.Core.Settings;

namespace Pingo.Core.Middleware
{
    public class ProtectLocalOnly
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<FiltersConfig> _settings;
        private readonly ILogger<OptOutOptInFilterProvider> _logger;
      
        public ProtectLocalOnly(RequestDelegate next, ProtectLocalOnlyOptions options, IOptions<FiltersConfig> settings, ILogger<OptOutOptInFilterProvider> logger)
        {
            _next = next;
            _settings = settings;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext, IAuthorizationService authorizationService)
        {
            bool isLocal = true;
            if (httpContext.Connection.RemoteIpAddress != null)
            {
                if (string.CompareOrdinal("::1", httpContext.Connection.RemoteIpAddress.ToString()) != 0)
                {
                    isLocal = false;
                }
            }
     
            if (!isLocal)
            {
                var paths = _settings.Value.MiddleWare.ProtectLocalOnly.Paths;
                if (paths.Any(path => httpContext.Request.Path.StartsWithSegments(path)))
                {
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
            }
            await _next(httpContext);
        }
    }
}