using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Antiforgery;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Pingo.Core;
using Pingo.Core.IoC;
using Pingo.Core.Middleware;
using Pingo.Core.Reflection;
using Pingo.Core.Settings;
using Serilog;
using Serilog.Sinks.RollingFile;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnvironment)
        {
            var RollingPath = Path.Combine(appEnvironment.ApplicationBasePath, "logs/myapp-{Date}.txt");
            Log.Logger = new LoggerConfiguration()
                 .WriteTo.RollingFile(RollingPath)
                .CreateLogger();
            Log.Information("Ah, there you are!");

            var schemaJsonPath = Path.Combine(appEnvironment.ApplicationBasePath,"appsettings-filters-schema.json");
            var schemaJson = File.ReadAllText(schemaJsonPath);
            JSchema schema = JSchema.Parse(schemaJson);

            var schemaJsonFiltersPath = Path.Combine(appEnvironment.ApplicationBasePath, "appsettings-filters.json");
            var schemaJsonFilters = File.ReadAllText(schemaJsonFiltersPath);
            JObject user = JObject.Parse(schemaJsonFilters);

            bool valid = user.IsValid(schema);
            if (!valid)
            {
                throw new Exception("Schema Validation Failed for appsettings-filters-schema.json");
            }

            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings-filters.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            // Initialize the global configuration static
            GlobalConfigurationRoot.Configuration = Configuration;
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
            });

            services.AddElm();
            services.ConfigureElm(options =>
            {
//                options.Path = new PathString("/foo");  // defaults to "/Elm"
                options.Filter = (name, level) => level >= LogLevel.Information;
            });
            // Add framework services.
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<Pingo.Authorization.Models.ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));

            //services.AddIdentity<ApplicationUser, IdentityRole>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders();

            services.AddIdentity<Pingo.Authorization.Models.ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<Pingo.Authorization.Models.ApplicationDbContext>()
                .AddDefaultTokenProviders();


            services.AddMvc();
            services.AddCaching(); // Memory Caching stuff

            // register the global configuration root 
            services.AddSingleton<IConfigurationRoot, GlobalConfigurationRoot>();

            services.AddOptions();
            services.Configure<FiltersConfig>(Configuration.GetSection(FiltersConfig.WellKnown_FilterSectionName));

            // Add application services.
         //   services.AddTransient<Pingo.Authorization.Services.IEmailSender, Pingo.Authorization.Services.AuthMessageSender>();
         //   services.AddTransient<Pingo.Authorization.Services.ISmsSender, Pingo.Authorization.Services.AuthMessageSender>();


            // Do this before we do a BuildServiceProvider because some downstream autofac modules need the librarymanager.
            ILibraryManager libraryManager = null;
            libraryManager = services.GetService<ILibraryManager>();
            TypeGlobals.LibraryManager = libraryManager;

            services.AddSingleton<IFilterProvider, Pingo.Core.Providers.OptOutOptInFilterProvider>();

            // autofac auto registration
            services.AddDependencies();
            var serviceProvider = services.BuildServiceProvider(Configuration);


            // Setup the PingoGlobal static .  Easier to use that trying to resolve everytime.
            var global = serviceProvider.GetService<Pingo.Core.Global>();
            Pingo.Core.TheApp.Global = global;
            return serviceProvider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IAntiforgery antiforgery, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCookieAuthentication(options =>
            {
                options.AutomaticAuthenticate = true;
            });
            app.UseProtectFolder(new ProtectFolderOptions
            {
                Path = "/Elm",
                PolicyName = "Authenticated"
            });

            app.UseAuthorizeMiddleware();
            app.UseElmPage(); // Shows the logs at the specified path
            app.UseElmCapture(); // Adds the ElmLoggerProvider

            loggerFactory.AddSerilog();
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
                        serviceScope.ServiceProvider.GetService<Pingo.Authorization.Models.ApplicationDbContext>()
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
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
