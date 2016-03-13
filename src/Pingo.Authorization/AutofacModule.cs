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

namespace Pingo.Authorization
{
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
