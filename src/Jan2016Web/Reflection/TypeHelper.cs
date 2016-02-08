using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.Extensions.PlatformAbstractions;

namespace Jan2016Web.Reflection
{
    public class TypeHelper<T>
    {
        public static ILibraryManager LibraryManager { get; set; }


        public static bool IsType(Type type)
        {
            return
                typeof (T).IsAssignableFrom(type) &&
                type.GetConstructor(Type.EmptyTypes) != null;
        }

        public static bool IsSubclassOf(Type type)
        {
            return (type != null && type.IsClass && type.IsSubclassOf(typeof (T)));
        }

        public static IEnumerable<Type> FindDerivedTypes(Assembly assembly)
        {
            var baseType = typeof (T);
            return assembly.GetTypes().Where(baseType.IsAssignableFrom);
        }


        public static IEnumerable<Type> FindTypesInAssembly(Assembly assembly, bool includeSubClass = false)
        {
            // Go through all assemblies referenced by the application and search for types matching a predicate
            IEnumerable<Type> typesSoFar = Type.EmptyTypes;
            Predicate<Type> predicate = TypeHelper<T>.IsType;
            Type[] typesInAsm;
            try
            {
                typesInAsm = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                typesInAsm = ex.Types;
            }
            typesSoFar = typesSoFar.Concat(typesInAsm);

            return
                typesSoFar.Where(
                    type =>
                        GenericTypeHelper.TypeIsPublicClass(type) &&
                        (predicate(type) || (!includeSubClass || IsSubclassOf(type))));
        }

        public static IEnumerable<Type> FindTypesInAssembly(Assembly assembly, Predicate<Type> predicate)
        {
            // Go through all assemblies referenced by the application and search for types matching a predicate
            IEnumerable<Type> typesSoFar = Type.EmptyTypes;

            Type[] typesInAsm;
            try
            {
                typesInAsm = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                typesInAsm = ex.Types;
            }
            typesSoFar = typesSoFar.Concat(typesInAsm);

            return typesSoFar.Where(type => GenericTypeHelper.TypeIsPublicClass(type) && predicate(type));
        }

        public static IEnumerable<Type> FindTypesInAssemblies(IEnumerable<Assembly> assemblies,
            Predicate<Type> predicate)
        {
            // Go through all assemblies referenced by the application and search for types matching a predicate
            IEnumerable<Type> typesSoFar = Type.EmptyTypes;

            foreach (Assembly assembly in assemblies)
            {
                Type[] typesInAsm;
                try
                {
                    typesInAsm = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    typesInAsm = ex.Types;
                }
                typesSoFar = typesSoFar.Concat(typesInAsm);
            }
            return typesSoFar.Where(type => GenericTypeHelper.TypeIsPublicClass(type) && predicate(type));
        }

        public static IEnumerable<Type> FindTypesInAssemblies(Predicate<Type> predicate)
        {
            var dap = new DefaultAssemblyProvider(LibraryManager);
            var assemblies = dap.CandidateAssemblies;
            return FindTypesInAssemblies(assemblies, predicate);
        }

        public static IEnumerable<Type> FindTypesWithCustomAttribute<TAttributeType>()
        {
            var registrationTypes = FindTypesInAssemblies(IsType).ToList();
            var types = new List<Type>();
            foreach (Type type in registrationTypes)
            {
                if (type.GetCustomAttributes(typeof (TAttributeType), true).Length > 0)
                {
                    types.Add(type);
                }
            }
            return types;
        }
    }
}
