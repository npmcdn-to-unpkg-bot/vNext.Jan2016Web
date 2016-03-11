using Microsoft.AspNet.Http;

namespace Pingo.Core.Middleware
{
    public abstract class MiddlewarePlugin : IMiddlewarePlugin
    {
        public abstract bool Invoke(HttpContext httpContext);
    }
}