using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using P6IdentityServer4.Services;

namespace P6IdentityServer4.Configuration
{
    public class RootObject
    {
        public List<Client> Clients { get; set; }
    }

    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddJsonClients(this IIdentityServerBuilder builder,
            string jsonPath)
        {
            var json = File.ReadAllText(jsonPath);

            var inMemoryJson = JsonConvert.DeserializeObject<RootObject>(json);

            IEnumerable<Client> clients = inMemoryJson.Clients;
            builder.Services.AddInstance(clients);
            builder.Services.AddTransient<IClientStore, JsonClientStore>();
            builder.Services.AddTransient<ICorsPolicyService, InMemoryCorsPolicyService>();
            return builder;
        }
    }
}
