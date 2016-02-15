using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Jan2016Web.Filters;
using Jan2016Web.IoC;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jan2016Web.Models;
using Jan2016Web.Reflection;
using Jan2016Web.Services;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Primitives;
using Pingo.Core.Settings;

namespace Jan2016Web
{

   
    public class Site 
    {
        public string Type { get; set; }
        public string Filter { get; set; }
        public RouteValues RouteValues { get; set; }
        public List<String> Areas { get; set; }
   //     public List<OptOut> OptOuts { get; set; }
    }

    public static class DependencyResolverExtensions
    {
        
        public static T GetService<T>(this HttpContext ctx) where T : class
        {
            return ctx.ApplicationServices.GetService(typeof(T)) as T;
        }

        public static T GetService<T>(this IApplicationBuilder applicationBuilder) where T : class
        {
            return applicationBuilder.ApplicationServices.GetService(typeof(T)) as T;
        }
        public static IApplicationBuilder UseAutofac(this IApplicationBuilder applicationBuilder)
        {
            ILibraryManager libraryManager = applicationBuilder.GetService<ILibraryManager>();
            return applicationBuilder;
        }
    }
    public class AutofacModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {

        }
    }
    public class TypeFinder
    {
        private static readonly Assembly _coreAssembly = typeof(TypeFinder).GetTypeInfo().Assembly;
        private static readonly AssemblyName _coreAssemblyName = new AssemblyName(_coreAssembly.FullName);

        private readonly ILibraryManager _libraryManager;

        public TypeFinder(ILibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        public Type FindType(string name, bool throwIfNotFound = false)
        {
            foreach (var assembly in GetLoadedAssemblies())
            {
                foreach (var candiate in assembly.ExportedTypes)
                {
                    if (string.Equals(name, candiate.FullName))
                    {
                        return candiate;
                    }
                }
            }

            if (!throwIfNotFound)
            {
                return null;
            }

            throw new InvalidOperationException($"Cannot find a type with the name '{name}'.");
        }
      
        public IEnumerable<Assembly> GetLoadedAssemblies()
        {
            var dap = new DefaultAssemblyProvider(_libraryManager);
            return dap.CandidateAssemblies;

            return _libraryManager.GetReferencingLibraries(_coreAssemblyName.Name)
                                  .Select(info => Assembly.Load(new AssemblyName(info.Name)));

            var dd = _libraryManager.GetLibraries();
            foreach (var ll in dd)
            {
                System.Diagnostics.Debug.WriteLine("Library:>>" + ll.Name);

            }

            var assemblies = dd.Select(info => Assembly.Load(new AssemblyName(info.Name)));
            return assemblies;
/*
            var referencingLibrary = _libraryManager.GetReferencingLibraries(_coreAssemblyName.Name);

            var many = _libraryManager.GetReferencingLibraries(_coreAssemblyName.Name)
                .SelectMany(info => info.Assemblies);
            foreach (var assemblyName in many)
            {
                System.Diagnostics.Debug.WriteLine("Assembly:>>" + assemblyName);

            }

            var result = referencingLibrary.Select(info =>
            {
                return Assembly.Load(new AssemblyName(info.Name));
            });
            return result;
//            return _libraryManager.GetReferencingLibraries(_coreAssemblyName.Name)
//                                  .Select(info => Assembly.Load(new AssemblyName(info.Name)));
*/
        }
    }

    public class GlobalSettings: IConfigurationRoot
    {
        public static IConfigurationRoot Configuration { get; set; }
        public IConfigurationSection GetSection(string key)
        {
            return Configuration.GetSection(key);
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return Configuration.GetChildren();
        }

        public IChangeToken GetReloadToken()
        {
            return Configuration.GetReloadToken();
        }

        public string this[string key]
        {
            get { return Configuration[key]; }
            set { Configuration[key] = value; }
        }

        public void Reload()
        {
            Configuration.Reload();
        }
    }
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            var dd = Configuration["Logging:SomeArray"];
            GlobalSettings.Configuration = Configuration;
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddSingleton<LogFilter>();

            services.AddMvc();
            services.AddCaching();

            // Create the Autofac container builder.
            //         var containerBuilder = new ContainerBuilder();
            // Add any Autofac modules or registrations.
            //         containerBuilder.RegisterModule(new AutofacModule());

//            return container.Resolve<IServiceProvider>();

            services.AddOptions();
            services.Configure<FiltersConfig>(Configuration.GetSection("Filters"));

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            // services.AddSingleton<IFilterProvider, MyFluentFilterProvider>();
            services.AddSingleton<IFilterProvider, OverrideFriendlyFilterProvider>();
            services.AddSingleton<IConfigurationRoot>();

            services.AddSingleton<IConfigurationRoot, GlobalSettings>();

            // var sp = services.BuildServiceProvider();

            //    containerBuilder.Populate(services);

            // autofac auto registration
            services.AddDependencies();
            var serviceProvider = services.BuildServiceProvider(Configuration);

            ILibraryManager libraryManager = null;
            libraryManager = services.GetService<ILibraryManager>();
            TypeGlobals.LibraryManager = libraryManager;
            var dependencyResolver = services.GetService<IDependencyResolver>();

            OverrideFriendlyFilterProvider.Configure(Configuration);

            var typeFinder = new TypeFinder(libraryManager);

            var registrationTypes =
                TypeHelper<Controller>.FindTypesInAssemblies(TypeHelper<Controller>.IsType).ToList();
            List<Type> types = new List<Type>();
            foreach (Type type in registrationTypes)
            {
                var attrib = type.GetCustomAttributes(typeof (AreaAttribute), true);
                if (attrib.Length > 0)
                {
                    RouteConstraintAttribute f = (RouteConstraintAttribute) attrib.First();

                    types.Add(type);
                }
            }

            //   var container = containerBuilder.Build();
            //   var sp = container.Resolve<IServiceProvider>();
            var authFilter = serviceProvider.GetService(typeof (Pingo.Filters.AuthActionFilter));
            var config = serviceProvider.GetService(typeof (IConfigurationRoot));
            var global = serviceProvider.GetService<Pingo.Core.Global>();
            Pingo.Core.TheApp.Global = global;
            return serviceProvider;

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ILibraryManager libraryManager = app.GetService<ILibraryManager>();
            TypeGlobals.LibraryManager = libraryManager;

            var typeFinder = new TypeFinder(libraryManager);

            var registrationTypes =
               TypeHelper<Controller>.FindTypesInAssemblies(TypeHelper<Controller>.IsType).ToList();
            List<Type> types = new List<Type>();
            foreach (Type type in registrationTypes)
            {
                var attrib = type.GetCustomAttributes(typeof (AreaAttribute), true);
                if (attrib.Length > 0)
                {
                    RouteConstraintAttribute f = (RouteConstraintAttribute) attrib.First();

                    types.Add(type);
                }
            }

            var areaTypes = TypeHelper<Controller>.FindTypesWithCustomAttribute<AreaAttribute>();

            var assemb = typeFinder.GetLoadedAssemblies();
            foreach (var a in assemb)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Assembly:>>" + a.FullName);
                }
                catch
                {
                }
            }
            app.UseMiddleware();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859
                try
                {
                    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                        .CreateScope())
                    {
                        serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                             .Database.Migrate();
                    }
                }
                catch { }
            }

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            app.UseStaticFiles();

            app.UseIdentity();

            // To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                   name: "areaRoute",
                   template: "{area:exists}/{controller}/{action}",
                   defaults: new { action = "Index" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
              
            });

            app.Use(async (context, next) =>
            {
                Console.WriteLine("1");
                await next();
                Console.WriteLine("8");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
