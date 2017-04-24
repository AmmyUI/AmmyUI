using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AmmySidekick
{
    public static class KnownTypes
    {
        private static readonly Dictionary<string, Type> TypesByFullName = new Dictionary<string, Type>();
        private static readonly Task LoadingTask;

        private static Assembly[] _assemblies;
        
        static KnownTypes()
        {
            var loading = GetAssemblies();
            LoadingTask = loading.ContinueWith(t => {
                if (!t.IsFaulted) {
                    _assemblies = t.Result;
                } else if (t.Exception != null) {
                    Debug.WriteLine("Unable to load assemblies " + t.Exception);
                } else {
                    Debug.WriteLine("Unable to load assemblies, unknown reason");
                }
            });
        }

        // This method is used by ExpressionBuilder as an alternative to `typeof` operator
        public static Type FindType<T>()
        {
            return typeof(T);
        }

        public static Type FindType(string typeName, object callerInstance)
        {
            LoadingTask.Wait();

            // NB! need to check if assembly list has changed
            if (TypesByFullName.Count == 0)
            {
#if WINDOWS_UWP || WINDOWS_PHONE_APP
                var executingAssemblyTypes = callerInstance.GetType().GetTypeInfo().Assembly.GetTypes();
#else
                var executingAssemblyTypes = callerInstance.GetType().Assembly.GetTypes();
#endif
                var allTypes = _assemblies.Where(a => a != null)
                                          .SelectMany(a => a.GetTypes())
                                          .Concat(executingAssemblyTypes);

                foreach (var type in allTypes)
                    TypesByFullName[type.FullName] = type;
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
                    var parmCount = parms.Count();
                    return parmCount == parameters.Length &&
                           parms.Zip(parameters, (pi, tp) => pi.ParameterType.IsAssignableFrom(tp)).All(b => b);
                });

            if (ctor == null)
                throw new InvalidOperationException("Couldn't find constructor for " + type + 
                                                    " with parameters: " + 
                                                    string.Join(", ", parameters.Select(t => t.Name)));

            return ctor;
        }

#if WINDOWS_UWP || WINDOWS_PHONE_APP
        private static async Task<Assembly[]> GetAssemblies()
        {
            var files = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFilesAsync() 
                        ?? new Windows.Storage.StorageFile[0];

            return files.Where(file => IsAssembly(file.FileType))
                        .Select(file => LoadAssembly(file.DisplayName))
                        .Where(a => a != null)
                        .ToArray();
        }

        private static Assembly LoadAssembly(string displayName)
        {
            try {
                return Assembly.Load(new AssemblyName(displayName));
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        private static bool IsAssembly(string fileType)
        {
            return fileType == ".dll" || fileType == ".exe";
        }
#else
        private static Task<Assembly[]> GetAssemblies()
        {
            var tcs = new TaskCompletionSource<Assembly[]>();
            tcs.SetResult(AppDomain.CurrentDomain.GetAssemblies());
            return tcs.Task;
        }
#endif
    }
}