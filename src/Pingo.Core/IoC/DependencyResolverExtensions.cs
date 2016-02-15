using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Configuration;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.PlatformAbstractions;

namespace Pingo.Core.IoC
{
    public static class DependencyResolverExtensions
    {
        public static T GetService<T>(this IServiceCollection services) where T : class
        {
            var result = (
                from ServiceDescriptor serviceDescriptor in services
                where serviceDescriptor.ServiceType == typeof (T)
                select serviceDescriptor);

            if (result.Any())
            {
                var first = result.First();
                return (T) first.ImplementationInstance;
            }
            return null;
        }

        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            DependencyResolver dependencyResolver = new DependencyResolver();
            dependencyResolver.Populate(services);
            ServiceDescriptor serviceDescriptor = new ServiceDescriptor(typeof(IDependencyResolver), dependencyResolver);
            services.TryAdd(serviceDescriptor);
            return services;
        }
        
        public static IServiceProvider BuildServiceProvider(this IServiceCollection services,
            IConfiguration configuration)
        {
            
            var libraryManager = services.GetService<ILibraryManager>();
            var dependencyResolver = services.GetService<IDependencyResolver>();

            var dap = new DefaultAssemblyProvider(libraryManager);

            IEnumerable<Assembly> assemblies = dap.CandidateAssemblies;
            dependencyResolver.RegisterModules(assemblies);

            ConfigurationModule configurationModule = new ConfigurationModule(configuration);
            dependencyResolver.RegisterModule(configurationModule);

            dependencyResolver.Build();

            IServiceProvider serviceProvider = dependencyResolver.Resolve<IServiceProvider>();
            return serviceProvider;
        }

        public static IApplicationBuilder UseDependencies(this IApplicationBuilder applicationBuilder, IConfiguration configuration)
        {
            IDependencyResolver dependencyResolver = applicationBuilder.GetService<IDependencyResolver>();
            if (dependencyResolver == null) return applicationBuilder;

            ILibraryManager libraryManager = applicationBuilder.GetService<ILibraryManager>();
            if (libraryManager == null) return applicationBuilder;

            IEnumerable<Assembly> assemblies = libraryManager.GetLoadableAssemblies(x=>x.Name.StartsWith("Pingo"));
            dependencyResolver.RegisterModules(assemblies);

            ConfigurationModule configurationModule = new ConfigurationModule(configuration);
            dependencyResolver.RegisterModule(configurationModule);

            dependencyResolver.Build();

            IServiceProvider serviceProvider = dependencyResolver.Resolve<IServiceProvider>();
            applicationBuilder.ApplicationServices = serviceProvider;

            return applicationBuilder;
        }

        public static IEnumerable<Assembly> GetLoadableAssemblies(this ILibraryManager libraryManager, Predicate<AssemblyName> predicate)
        {
            List<Assembly> result = new List<Assembly>();

            IEnumerable<Library> libraries = libraryManager.GetLibraries();

            IEnumerable<AssemblyName> assemblyNames = libraries.SelectMany(e => e.Assemblies).Distinct();
            assemblyNames = Enumerable.Where(assemblyNames, e => predicate(e));

            foreach (AssemblyName assemblyName in assemblyNames)
            {
                Assembly assembly = Assembly.Load(assemblyName);
                result.Add(assembly);
            }

            return result;
        }

        public static T GetService<T>(this IApplicationBuilder applicationBuilder) where T : class
        {
            return applicationBuilder.ApplicationServices.GetService(typeof(T)) as T;
        }
    }
}