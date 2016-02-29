using System;
using System.Reflection;
using Autofac;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using Pingo.Core.Reflection;
using Serilog;
using Module = Autofac.Module;
using Pingo.Core.Attributes;
using Pingo.Core.IoC;
using Pingo.Core.Startup;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Pingo.Core;

namespace Pingo.Authorization
{
    public class MyConfigureServicesRegistrant : ConfigureServicesRegistrant
    {
        public override void OnConfigureServices(IServiceCollection services)
        {
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<Pingo.Authorization.Models.ApplicationDbContext>(options =>
                    options.UseSqlServer(
                        GlobalConfigurationRoot.Configuration["Data:DefaultConnection:ConnectionString"]));

            services.AddIdentity<Models.ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<Models.ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }
    }

    public class AutofacModule : Module
    {
        static ILogger logger = Log.ForContext<AutofacModule>();

        protected override void Load(ContainerBuilder builder)
        {
            logger.Information("Hi from Pingo.Authorization Autofac.Load!");
            var assembly = this.GetType().GetTypeInfo().Assembly;
            builder.AutoRegisterServiceRegistrants(assembly);
        }
    }
}
