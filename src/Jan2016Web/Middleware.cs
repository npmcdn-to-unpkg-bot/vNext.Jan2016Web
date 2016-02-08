using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Compilation;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.PlatformAbstractions;

namespace Jan2016Web
{
    // You may need to install the Microsoft.AspNet.Http.Abstractions package into your project
    public class Middleware
    {
        private readonly RequestDelegate _next;

        public Middleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            ILibraryManager libraryManager = httpContext.GetService<ILibraryManager>();
            var typeFinder = new TypeFinder(libraryManager);
            var assemb = typeFinder.GetLoadedAssemblies();
            foreach (var a in assemb)
            {
                System.Diagnostics.Debug.WriteLine("Assembly:>>" + a.FullName);
            }
            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Middleware>();
        }
    }
}
