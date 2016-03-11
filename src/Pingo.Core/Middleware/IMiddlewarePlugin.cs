using Microsoft.AspNet.Http;

namespace Pingo.Core.Middleware
{
    public interface IMiddlewarePlugin
    {
        bool Invoke(HttpContext httpContext);
    }
}