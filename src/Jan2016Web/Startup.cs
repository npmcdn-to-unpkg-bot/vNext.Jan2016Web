using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Jan2016Web.Filters;
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
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;


namespace Jan2016Web
{
    public class RouteValues
    {
        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }

    }
    public class OptInOutRecord
    {
        public List<String> Areas { get; set; }

        private Dictionary<String, bool> _areasMap;
        public Dictionary<String, bool> AreasMap
        {
            get
            {
                if (_areasMap == null)
                {
                    _areasMap = new Dictionary<string, bool>();
                    if (Areas != null)
                    {
                        foreach (var item in Areas)
                        {
                            _areasMap.Add(item, true);
                        }
                        Areas = null;// to reduce memory
                    }
                }
                return _areasMap;
            } 
        }

        public List<String> Controllers { get; set; }
        private Dictionary<String, bool> _controllersMap;
        public Dictionary<String, bool> ControllersMap
        {
            get
            {
                if (_controllersMap == null)
                {
                    _controllersMap = new Dictionary<string, bool>();
                    if (Controllers != null)
                    {
                        foreach (var item in Controllers)
                        {
                            _controllersMap.Add(item, true);
                        }
                        Controllers = null;
                    }
                }
                return _controllersMap;
            }
        }

        public List<String> Actions { get; set; }
        private Dictionary<String, bool> _actionsMap;
        public Dictionary<String, bool> ActionsMap
        {
            get
            {
                if (_actionsMap == null)
                {
                    _actionsMap = new Dictionary<string, bool>();
                    if (Actions != null)
                    {
                        foreach (var item in Actions)
                        {
                            _actionsMap.Add(item, true);
                        }
                        Actions = null;
                    }
                }
                return _actionsMap;
            }
        }
    }

    public class AuthorizationConfig
    {
        public string Filter { get; set; }
        public RouteValues RouteValues { get; set; }
        public OptInOutRecord OptOut { get; set; }
    }

    public class SimpleManyRecord
    {
        public string Filter { get; set; }
        public OptInOutRecord Value { get; set; }
    }


    public class SimpleManyConfig
    {
        public List<SimpleManyRecord> OptOut { get; set; }
        public List<SimpleManyRecord> OptIn { get; set; }
    }


    public class FiltersConfig
    {
        public AuthorizationConfig Authorization { get; set; }
        public SimpleManyConfig SimpleMany { get; set; }
    }

   
   
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
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
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

            services.AddOptions();
            services.Configure<FiltersConfig>(Configuration.GetSection("Filters"));

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddSingleton<IFilterProvider, OverrideFriendlyFilterProvider>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ILibraryManager libraryManager = app.GetService<ILibraryManager>();
            var dd = new DefaultAssemblyProvider(libraryManager);
            var cand = dd.CandidateAssemblies;
            foreach (var a in cand)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Assembly:>>" + a.FullName);
                }
                catch
                {
                }
            }

            var typeFinder = new TypeFinder(libraryManager);

            TypeHelper<Controller>.LibraryManager = app.GetService<ILibraryManager>();
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
