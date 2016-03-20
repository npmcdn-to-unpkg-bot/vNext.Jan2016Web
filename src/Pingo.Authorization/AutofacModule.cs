#define ENTITY_IDENTITY
#undef ENTITY_IDENTITY

using System;
using System.Reflection;
using Autofac;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Module = Autofac.Module;
using Pingo.Core.IoC;
using Pingo.Core.Startup;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using P6.AspNet.CassandraIdentity3;
using Pingo.Authorization.Models;

namespace Pingo.Authorization
{
#if ENTITY_IDENTITY
    public class MyConfigureEntityFrameworkRegistrant : ConfigureEntityFrameworkRegistrant
    {
        public override void OnAddDbContext(EntityFrameworkServicesBuilder builder)
        {
            builder.AddDbContext<Pingo.Authorization.Models.ApplicationDbContext>(options =>
                options.UseInMemoryDatabase());

            /*
                        builder.AddDbContext<Pingo.Authorization.Models.ApplicationDbContext>(options =>
                                options.UseSqlServer(
                                    GlobalConfigurationRoot.Configuration["Data:DefaultConnection:ConnectionString"]));
                                    */
        }
    }
#endif
    public class MyConfigureServicesRegistrant : ConfigureServicesRegistrant
    {
        public override void OnConfigureServices(IServiceCollection services)
        {
            var builder = services.AddEntityFramework();
            //builder.AddSqlServer();
            builder.AddInMemoryDatabase();
            builder.AddAllConfigureEntityFrameworkRegistrants();
            /*
                            .AddDbContext<Pingo.Authorization.Models.ApplicationDbContext>(options =>
                                options.UseSqlServer(
                                    GlobalConfigurationRoot.Configuration["Data:DefaultConnection:ConnectionString"]));
            */
#if ENTITY_IDENTITY
            services.AddIdentity<Models.ApplicationUser, IdentityRole>()
                            .AddEntityFrameworkStores<Models.ApplicationDbContext>()
                            .AddDefaultTokenProviders();
#else
            services.AddIdentity<ApplicationUser, CassandraIdentityRole>()
                .AddCassandraIdentityStores<ApplicationDbContext, Models.ApplicationUser, CassandraIdentityRole, Guid>()
                .AddDefaultTokenProviders();
#endif
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
