using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Builder;

namespace Pingo.Core.Middleware
{
    // You may need to install the Microsoft.AspNet.Http.Abstractions package into your project

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ProtectPathExtensions
    {
        public static IApplicationBuilder UseProtectPath(this IApplicationBuilder builder, ProtectPathOptions options)
        {
            return builder.UseMiddleware<ProtectPath>(options);
        }
    }
}
