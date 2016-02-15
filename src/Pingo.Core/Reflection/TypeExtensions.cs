using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Infrastructure;

namespace Pingo.Core.Reflection
{
    public static class TypeExtensions
    {
        public static bool IsPublicClass(this Type type)
        {
            return (type != null
                    && type.GetTypeInfo().IsPublic
                    && type.GetTypeInfo().IsClass
                    && !type.GetTypeInfo().IsAbstract);
        }

        public static IEnumerable<Type> WithCustomAttribute<TAttributeType>(this IEnumerable<Type> master)
        {
            return
                master.Where(type => type.GetTypeInfo().GetCustomAttributes(typeof (TAttributeType), true).Any())
                    .ToList();
        }

        public static RouteConstraintAttribute GetRouteConstraintAttribute<TAttributeType>(this Type type)
        {
            var attrib = type.GetTypeInfo().GetCustomAttributes(typeof (TAttributeType), true);
            if (attrib.Any())
            {
                return (RouteConstraintAttribute) attrib.First();
            }
            return null;
        }


    }
}
