using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.InMemory;
using Microsoft.Extensions.DependencyInjection;
using P6IdentityServer4.Services;

namespace P6IdentityServer4.Configuration
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddJsonClients(this IIdentityServerBuilder builder,
            IEnumerable<Client> clients)
        {
            builder.Services.AddInstance(clients);
            builder.Services.AddTransient<IClientStore, JsonClientStore>();
            builder.Services.AddTransient<ICorsPolicyService, InMemoryCorsPolicyService>();
            return builder;
        }
    }
}
