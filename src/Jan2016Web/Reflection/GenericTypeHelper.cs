using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jan2016Web.Reflection
{
    public static class GenericTypeHelper
    {
        public static bool TypeIsPublicClass(Type type)
        {
            return (type != null && type.IsPublic && type.IsClass && !type.IsAbstract);
        }
    }
}
