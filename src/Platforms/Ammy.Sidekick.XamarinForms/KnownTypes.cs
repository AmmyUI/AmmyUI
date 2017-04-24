using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AmmySidekick
{
    public static class KnownTypes
    {
        private static readonly Dictionary<string, Type> TypesByFullName = new Dictionary<string, Type>();
        private static readonly Assembly[] Assemblies;

        static KnownTypes()
        {
            Assemblies = GetAssemblies();
        }

        public static Type FindType<T>()
        {
            return typeof(T);
        }

        public static Type FindType(string typeName)
        {
            if (TypesByFullName.Count == 0)
            {
                var allTypes = Assemblies.Where(a => a != null)
                                         .SelectMany(a => a.DefinedTypes);

                foreach (var type in allTypes)
                    TypesByFullName[type.FullName] = type.AsType();
            }

            Type ret;
            if (TypesByFullName.TryGetValue(typeName, out ret)) return ret;

            throw new InvalidOperationException("Couldn't find type: " + typeName);
        }

        public static ConstructorInfo GetConstructor(Type type, Type[] parameters)
        {
            var ctor = type.GetConstructors()
                           .FirstOrDefault(ci => {
                               var parms = ci.GetParameters();
                               var parmCount = parms.Length;

                               return parmCount == parameters.Length &&
                                      parms.Zip(parameters, (pi, tp) => pi.ParameterType.IsAssignableFrom(tp)).All(b => b);
                           });

            if (ctor == null)
                throw new InvalidOperationException("Couldn't find constructor for " + type + 
                                                    " with parameters: " + 
                                                    string.Join(", ", parameters.Select(t => t.Name)));

            return ctor;
        }

        private static Assembly[] GetAssemblies()
        {
            var typeInfo = typeof(string).GetTypeInfo();
            var assembly = typeInfo.Assembly;
            var type = assembly.GetType("System.AppDomain");
            var currentDomainProp = type.GetRuntimeProperty("CurrentDomain");
            var getMethod = currentDomainProp.GetMethod;
            var currentdomain = getMethod.Invoke(null, new object[] {});
            var getassemblies = currentdomain.GetType().GetRuntimeMethod("GetAssemblies", new Type[] { });
            return getassemblies.Invoke(currentdomain, new object[] { }) as Assembly[];
        }
    }
}