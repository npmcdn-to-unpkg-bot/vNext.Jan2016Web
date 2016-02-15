using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.Extensions.PlatformAbstractions;

namespace Jan2016Web.Reflection
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> WithCustomAttribute<TAttributeType>(this IEnumerable<Type> master)
        {
            return master.Where(type => type.GetCustomAttributes(typeof (TAttributeType), true).Length > 0).ToList();
        }
        public static RouteConstraintAttribute GetRouteConstraintAttribute<TAttributeType>(this Type type)
        {
            var attrib = type.GetCustomAttributes(typeof(TAttributeType), true);
            if (attrib.Length > 0)
            {
                return (RouteConstraintAttribute) attrib.First();
            }
            return null;
        }
    }

    public class TypeGlobals
    {
        public static ILibraryManager LibraryManager { get; set; }
    }
    public class TypeHelper<T>: TypeGlobals
    {
   
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

        public static Assembly FindAssembyByName(string name)
        {
            var dap = new DefaultAssemblyProvider(LibraryManager);
            var assemblies = dap.CandidateAssemblies;
            var result = (
                from Assembly assembly in assemblies
                let assemblyName = assembly.GetName()
                where System.String.Compare(name, assemblyName.Name, System.StringComparison.OrdinalIgnoreCase) == 0
                select assembly);
            return result.FirstOrDefault();
        }

        /*
         * [full namespaced path to Class], [name of assembly]

i.e. COA.MEF.Calculator.Globalization.Views.Home, COA.MEF.Calculator.Globalization
         * */
        /// <summary>
        ///  Get type by full name 
        /// </summary>
        /// <param name="fullType">[full namespaced path to Class], [name of assembly]  i.e. COA.MEF.Calculator.Globalization.Views.Home, COA.MEF.Calculator.Globalization </param>
        /// <returns></returns>
        public static Type GetTypeByFullName(string fullType)
        {
            var splitFullType = fullType.Split(',');
            var typeName = splitFullType[0].TrimEnd().TrimStart();
            var assemblyName = splitFullType[1].TrimEnd().TrimStart();
            var assembly = FindAssembyByName(assemblyName);
            var type = assembly?.GetType(typeName);
            return type;
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
