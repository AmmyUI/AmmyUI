using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            try {
                Assemblies = GetAssemblies();
            } catch {
                Debug.WriteLine("Failed to load assemblies");

                Assemblies = new Assembly[] {
                    typeof(string).GetTypeInfo().Assembly
                };
            }
        }

        public static Tuple<Type, FieldInfo> FindShortTypeWithProperty(string typeName, string property)
        {
            EnsureTypesLoaded();

            foreach (var kvp in TypesByFullName) {
                if (kvp.Key.EndsWith("." + typeName)) {
                    var foundProperty = FindBindablePropertyField(kvp.Value, property);
                    if (foundProperty != null)
                        return Tuple.Create(kvp.Value, foundProperty);
                }
            }

            return null;
        }

        public static Type FindType(string typeName)
        {
            EnsureTypesLoaded();

            Type ret;
            if (TypesByFullName.TryGetValue(typeName, out ret))
                return ret;

            // If searching assemblies failed, try to resolve type manually
            return ManualFindType(typeName);
        }

        private static void EnsureTypesLoaded()
        {
            if (TypesByFullName.Count == 0) {
                var allTypes = Assemblies.Where(a => a != null)
                                         .SelectMany(a => a.DefinedTypes);

                foreach (var type in allTypes)
                    TypesByFullName[type.FullName] = type.AsType();
            }
        }

        private static Type ManualFindType(string typeName)
        {
            var uwpNetAssemblies = new[] {
                "System.Net.Sockets, Version=4.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                "System.Net.Sockets, Version=4.3.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                "System.Net.Primitives, Version=4.0.11.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                "System.Net.Primitives, Version=4.3.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                "System.Net.Primitives, Version=4.0.10.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            };

            if (string.IsNullOrWhiteSpace(typeName))
                throw new InvalidOperationException("Type name shouldn't be empty or null");

            if (typeName.StartsWith("System.Net", StringComparison.OrdinalIgnoreCase)) {
                foreach (var netAssembly in uwpNetAssemblies) {
                    try {
                        var type = Type.GetType(typeName + ", " + netAssembly);
                        if (type != null)
                            return type;
                    } catch {
                        // do nothing if load failed
                    }
                }
            }

            return null;
        }

        public static FieldInfo FindBindablePropertyField(Type type, string propertyName)
        {
            var typeInfo = type.GetTypeInfo();

            foreach (var declaredField in typeInfo.DeclaredFields)
                if (declaredField.IsStatic && declaredField.Name == propertyName + "Property")
                    return declaredField;

            if (typeInfo.BaseType == null)
                return null;

            return FindBindablePropertyField(typeInfo.BaseType, propertyName);
        }

        public static ConstructorInfo GetConstructor(Type type, params Type[] parameters)
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
            var currentdomain = getMethod.Invoke(null, new object[] { });
            var getassemblies = currentdomain.GetType().GetRuntimeMethod("GetAssemblies", new Type[] { });
            return getassemblies.Invoke(currentdomain, new object[] { }) as Assembly[];
        }
    }
}