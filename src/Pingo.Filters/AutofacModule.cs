using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNet.Mvc.Filters;
using Pingo.Core.Reflection;
using Module = Autofac.Module;

namespace Pingo.Filters
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var derivedTypes = TypeHelper<ActionFilterAttribute>.FindDerivedTypes(assembly).ToArray();
            builder.RegisterTypes(derivedTypes);
            /*

            builder.RegisterType<AuthActionFilter>().SingleInstance();
            builder.RegisterType<LogFilter>().SingleInstance();
            builder.RegisterType<LogFilter2>().SingleInstance();
            builder.RegisterType<LogFilter3>().SingleInstance();
            */
        }
    }
}
