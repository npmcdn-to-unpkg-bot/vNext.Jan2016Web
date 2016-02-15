using System.Collections.Generic;
using System.Reflection;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Pingo.Core.IoC
{
    public interface IDependencyResolver
    {
        void RegisterModule(IModule module);
        void RegisterModules(IEnumerable<Assembly> assemblies);
        void Populate(IServiceCollection services);
        void Build();
        T Resolve<T>() where T : class;
    }
}