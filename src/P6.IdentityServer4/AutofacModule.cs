using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using P6IdentityServer4.Configuration;
using P6IdentityServer4.IdentityServerApp.Configuration;
using P6IdentityServer4.IdentityServerApp.Extensions;
using Pingo.Core;
using Pingo.Core.IoC;
using Pingo.Core.Settings;
using Pingo.Core.Startup;
using Serilog;
using Module = Autofac.Module;

namespace P6IdentityServer4
{



    public class MyConfigureEntityFrameworkRegistrant : ConfigureEntityFrameworkRegistrant
    {
        public override void OnAddDbContext(EntityFrameworkServicesBuilder builder)
        {

         
        }
    }
    public class MyConfigureServicesRegistrant : ConfigureServicesRegistrant
    {
        public override void OnConfigureServices(IServiceCollection services)
        {
            // Can't seem to get an IOptions thing here.
            //var fc = services.GetService<IOptions<FiltersConfig>>();
            var _hostingEnvironment = services.GetService<IHostingEnvironment>();
            var _appEnvironment = services.GetService<IApplicationEnvironment>();

            var jsonFilePath = Path.Combine(_appEnvironment.ApplicationBasePath, "App_Data/IdentityServer4.Clients.json");
            var cert = new X509Certificate2(Path.Combine(_appEnvironment.ApplicationBasePath, "idsrv3test.pfx"), "idsrv3test");
            var builder = services.AddIdentityServer(options =>
            {
                options.SigningCertificate = cert;
            });
            builder.AddJsonClients(jsonFilePath);
            builder.AddInMemoryScopes(Scopes.Get());
            builder.AddInMemoryUsers(Users.Get());

            builder.AddCustomGrantValidator<CustomGrantValidator>();

        }
    }

    public class MyConfigureRegistrant : ConfigureRegistrant
    {
        public override void OnConfigure(IApplicationBuilder app)
        {
            var fc = app.GetService<IOptions<FiltersConfig>>();
            var _hostingEnvironment = app.GetService<IHostingEnvironment>();
            var _appEnvironment = app.GetService<IApplicationEnvironment>();
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
