using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNet.Mvc.Filters;
using Pingo.Core.Reflection;
using Serilog;
using Module = Autofac.Module;

namespace Pingo.Filters
{
    public class AutofacModule : Module
    {
        static ILogger logger = Log.ForContext<AutofacModule>();
        protected override void Load(ContainerBuilder builder)
        {
            logger.Information("Hi from pingo.filters Autofac.Load!");
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var derivedTypes = TypeHelper<ActionFilterAttribute>.FindDerivedTypes(assembly).ToArray();
            var derivedTypesName = derivedTypes.Select(x => x.GetTypeInfo().Name);
            logger.Information("Found these types: {DerivedTypes}", derivedTypesName);

            builder.RegisterTypes(derivedTypes).SingleInstance();
            /*

            builder.RegisterType<AuthActionFilter>().SingleInstance();
            builder.RegisterType<LogFilter>().SingleInstance();
            builder.RegisterType<LogFilter2>().SingleInstance();
            builder.RegisterType<LogFilter3>().SingleInstance();
            */
        }
    }
}
