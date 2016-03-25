#define ENTITY_IDENTITY
//#undef ENTITY_IDENTITY
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using Basic.Swagger;
using Microsoft.AspNet.Antiforgery;
using Microsoft.AspNet.Authentication.DeveloperAuth;
using Microsoft.AspNet.Authentication.Twitter2;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Pingo.Core;
using Pingo.Core.IoC;
using Pingo.Core.Middleware;
using Pingo.Core.Reflection;
using Pingo.Core.Startup;
using Serilog;
using Serilog.Sinks.RollingFile;
using Swashbuckle.SwaggerGen.Generator;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.OptionsModel;
using p6.CassandraStore.Settings;


namespace WebApplication1
{
    public class Startup
    {
        public static bool isValidJson(string jsonPath, string schemaJsonPath)
        {

            var schemaJson = File.ReadAllText(schemaJsonPath);
            JSchema schema = JSchema.Parse(schemaJson);
            var json = File.ReadAllText(jsonPath);
            JObject jsonO = JObject.Parse(json);

            bool valid = jsonO.IsValid(schema);
            return valid;
        }

        private readonly IApplicationEnvironment _appEnvironment;
        private readonly IHostingEnvironment _hostingEnvironment;
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
            _hostingEnvironment = env;

            var RollingPath = Path.Combine(appEnvironment.ApplicationBasePath, "logs/myapp-{Date}.txt");
            Log.Logger = new LoggerConfiguration()
                .WriteTo.RollingFile(RollingPath)
                .CreateLogger();
            Log.Information("Ah, there you are!");

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
            services.AddInstance<IHostingEnvironment>(_hostingEnvironment);
            services.AddInstance<IApplicationEnvironment>(_appEnvironment);
            services.AddOptions();

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

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            services.AddMvcCore().AddJsonFormatters();
            services.AddSwaggerGen(c =>
            {
                c.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Swashbuckle Sample API",
                    Description = "A sample API for testing Swashbuckle",
                    TermsOfService = "Some terms ..."
                });

                c.DescribeAllEnumsAsStrings();

                c.OperationFilter<AssignOperationVendorExtensions>();
             
            });

            if (_hostingEnvironment.IsDevelopment())
            {
                services.ConfigureSwaggerGen(c =>
                {
                    var xmlPath = string.Format(@"{0}\artifacts\bin\WebApplication1\{1}\{2}{3}\WebApplication1.xml",
                        GetSolutionBasePath(),
                        _appEnvironment.Configuration,
                        _appEnvironment.RuntimeFramework.Identifier,
                        _appEnvironment.RuntimeFramework.Version.ToString().Replace(".", string.Empty));
                   
                    c.IncludeXmlComments(xmlPath);

                    var xmlPath2 = string.Format(@"{0}\artifacts\bin\p6.api.animals.v1\{1}\dotnet5.4\p6.api.animals.v1.xml",
                       GetSolutionBasePath(),
                       _appEnvironment.Configuration);

                    c.IncludeXmlComments(xmlPath2);

                });
            }

            services.AddLogging();
            services.AddWebEncoders();
            services.AddCors();

            services.AddCaching(); // Memory Caching stuff
            services.AddSession();

            // register the global configuration root 
            services.AddSingleton<IConfigurationRoot, GlobalConfigurationRoot>();

            services.Configure<CassandraConfig>(Configuration.GetSection(CassandraConfig.WellKnown_SectionName));


            // Add application services.

            // Do this before we do a BuildServiceProvider because some downstream autofac modules need the librarymanager.
            ILibraryManager libraryManager = null;
            libraryManager = services.GetService<ILibraryManager>();
            TypeGlobals.LibraryManager = libraryManager;

            services.AddSingleton<IFilterProvider, Pingo.Core.Providers.OptOutOptInFilterProvider>();

            services.AddTransient<ClaimsPrincipal>(
               s => s.GetService<IHttpContextAccessor>().HttpContext.User);

            services.Configure<IdentityOptions>(options =>
            {
                options.Cookies.ApplicationCookie.LoginPath = new Microsoft.AspNet.Http.PathString("/Identity/Account/Login");
                options.Cookies.ApplicationCookie.LogoutPath = new Microsoft.AspNet.Http.PathString("/Identity/Account/LogOff");
            });

            services.AddAllConfigureServicesRegistrants(Configuration);
            // autofac auto registration
            services.AddDependencies();
            var serviceProvider = services.BuildServiceProvider(Configuration);

            // Setup the PingoGlobal static .  Easier to use that trying to resolve everytime.
            var global = serviceProvider.GetService<Pingo.Core.Global>();
            Pingo.Core.TheApp.Global = global;
            return serviceProvider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IAntiforgery antiforgery, IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            app.UseIdentity() 
                .UseDeveloperAuthAuthentication(
                    new DeveloperAuthOptions()
                    {
                        ConsumerKey = "uWkHwFNbklXgsLHYzLfRXcThw",
                        ConsumerSecret = "2kyg9WdUiJuU2HeWYJEuvwzaJWoweLadTgG3i0oHI5FeNjD5Iv"
                    })
                .UseTwitter2Authentication(
                    new Twitter2Options()
                    {
                        ConsumerKey = "uWkHwFNbklXgsLHYzLfRXcThw",
                        ConsumerSecret = "2kyg9WdUiJuU2HeWYJEuvwzaJWoweLadTgG3i0oHI5FeNjD5Iv"
                    });

            app.AddAllConfigureRegistrants();
            app.UseCookieAuthentication(options =>
            {
                options.LoginPath = new PathString("/Identity/Account/Login");
                options.LogoutPath = new PathString("/Identity/Account/LogOff");
            });

            /*
            app.UseProtectFolder(new ProtectFolderOptions
            {
                Path = "/Elm",
                PolicyName = "Authenticated"
            });
            */
            app.UseProtectLocalOnly(new ProtectLocalOnlyOptions());
            app.UseProtectPath(new ProtectPathOptions
            {
                PolicyName = "Authenticated"
            });

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
#if ENTITY_IDENTITY
                        serviceScope.ServiceProvider.GetService<Pingo.Authorization.Models.ApplicationDbContext>()
                            .Database.Migrate();
#endif
                    }
                }
                catch
                {
                }
            }

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            app.UseIdentityServer();

            app.UseCors(policy =>
            {
                policy.WithOrigins(
                    "http://localhost:28895",
                    "http://localhost:14016",
                    "http://localhost:7017");

                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
            });
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            app.UseIdentityServerAuthentication(options =>
            {
                options.Authority = WebApplication1.IdentityServerClients.Configuration.Constants.BaseAddress;
                options.ScopeName = "api1";
                options.ScopeSecret = "secret";

                options.AutomaticAuthenticate = true;
                options.AutomaticChallenge = true;
            });

            app.UseStaticFiles();


            // IMPORTANT: This session call MUST go before UseMvc()
            app.UseSession();

            // To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areaRoute",
                    template: "{area:exists}/{controller}/{action}",
                    defaults: new {action = "Index"});

                routes.MapRoute(
                    name: "default",
                    template: "{area=Main}/{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwaggerGen();
            app.UseSwaggerUi();
        }

        private string GetSolutionBasePath()
        {
            var dir = Directory.CreateDirectory(_appEnvironment.ApplicationBasePath);
            while (dir.Parent != null)
            {
                if (dir.GetFiles("global.json").Any())
                    return dir.FullName;

                dir = dir.Parent;
            }
            throw new InvalidOperationException("Failed to detect solution base path - global.json not found.");
        }
        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }

}
