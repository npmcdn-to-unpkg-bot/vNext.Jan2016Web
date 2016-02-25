using System;
using System.Reflection;
using Autofac;
using Pingo.Core.Reflection;
using Serilog;
using Module = Autofac.Module;
using Pingo.Core.Attributes;
using Pingo.Core.IoC;

namespace Pingo.Authorization
{
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
