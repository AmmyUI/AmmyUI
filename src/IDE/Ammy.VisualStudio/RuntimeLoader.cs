using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.TextManager.Interop;
using VSLangProj;
using IServiceProvider = System.IServiceProvider;

namespace Ammy.VisualStudio
{
    public static class RuntimeLoader
    {
        private static readonly ConcurrentDictionary<string, Project> ProjectCache = new ConcurrentDictionary<string, Project>();
        private static Assembly _serviceAssembly;
        private static DateTime _lastError;

        public static ITagger<T> CreateErrorTagger<T>(string filePath, IServiceProvider serviceProvider, ITextBuffer buffer) where T : ITag
        {
            return GetObject<ITagger<T>>(filePath, t => typeof(ITagger<T>).IsAssignableFrom(t), serviceProvider, buffer, serviceProvider);
        }
        
        public static IWpfTextViewMargin CreateTextViewMargin(string filePath, IServiceProvider serviceProvider, IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            return GetObject<IWpfTextViewMargin>(filePath, t => typeof(IWpfTextViewMargin).IsAssignableFrom(t), serviceProvider, wpfTextViewHost, marginContainer, serviceProvider);
        }

        public static ICompletionSource CreateCompletionSource(string filePath, IServiceProvider serviceProvider, ITextBuffer textBuffer)
        {
            return GetObject<ICompletionSource>(filePath, t => typeof(ICompletionSource).IsAssignableFrom(t), serviceProvider, textBuffer, serviceProvider);
        }

        public static IClassifier GetClassifier(string filePath, IServiceProvider serviceProvider, ITextBuffer buffer, ITextDocument textDocument, IClassificationTypeRegistryService classificationRegistry, IClassificationFormatMapService classificationFormatMapService)
        {
            return GetObject<IClassifier>(filePath, t => typeof(IClassifier).IsAssignableFrom(t), serviceProvider, buffer, textDocument, classificationRegistry, classificationFormatMapService, serviceProvider);
        }

        public static IOleCommandTarget CreateCommandHandler(string filePath, IServiceProvider serviceProvider, IVsTextView textViewAdapter, ITextView textView, ITextStructureNavigatorSelectorService navigatorService, ISignatureHelpBroker signatureHelpBroker, ICompletionBroker completionBroker)
        {
            return GetObject<IOleCommandTarget>(filePath, t => typeof(IOleCommandTarget).IsAssignableFrom(t), serviceProvider, textViewAdapter, textView, serviceProvider, navigatorService, signatureHelpBroker, completionBroker);
        }

        public static T GetObjectByName<T>(string filePath, string name, IServiceProvider serviceProvider, params object[] parameters) where T : class
        {
            return GetObject<T>(filePath, t => t.Name == name, serviceProvider, parameters);
        }

        public static T GetObject<T>(string filePath, Func<Type, bool> predicate, IServiceProvider serviceProvider, params object[] parameters) where T : class 
        {
            try {
                var assembly = GetServiceAssembly(filePath, serviceProvider);

                if (assembly == null)
                    return null;

                var type = assembly.GetTypes()
                                   .FirstOrDefault(predicate);

                if (type == null)
                    return null;

                var obj = (T) Activator.CreateInstance(type, parameters);
                var componentModel = (IComponentModel) serviceProvider.GetService(typeof (SComponentModel));

                componentModel.DefaultCompositionService.SatisfyImportsOnce(obj);

                return obj;
            } catch (Exception e) {
                if ((DateTime.Now - _lastError) > TimeSpan.FromMinutes(1))
                    MessageBox.Show(e.Message);

                _lastError = DateTime.Now;

                return null;
            }
        }

        private static Assembly GetServiceAssembly(string filePath, IServiceProvider serviceProvider)
        {
            if (_serviceAssembly != null)
                return _serviceAssembly;

            var dte = (DTE)serviceProvider.GetService(typeof(DTE));
            var project = GetProjectByFilename(dte, filePath, serviceProvider);

            if (project == null)
                return null;

            var assemblyDirectory = GetAssemblyDirectory(project, dte);
            var serviceAssemblyPath = Path.Combine(assemblyDirectory, "Ammy.VisualStudio.Service.dll");

            return _serviceAssembly = Assembly.LoadFrom(serviceAssemblyPath);
        }

        private static string GetAssemblyDirectory(Project project, DTE dte)
        {
            var nl = Environment.NewLine;
            var proj = (VSProject)project.Object;
            var sidekickReference = proj.References.Cast<Reference>().FirstOrDefault(r => r.Path.EndsWith("AmmySidekick.dll", StringComparison.InvariantCultureIgnoreCase));
            string packagesPath = null;

            if (sidekickReference != null) {
                var match = Regex.Match(sidekickReference.Path, @"(.+packages\\Ammy)\.\w+((?:\.\d+){3})");
                if (match.Success && match.Groups.Count >= 3) {
                    var version = match.Groups[2].Value;
                    packagesPath = match.Groups[1].Value + version + "\\build";
                    EnsureVersion(version);
                } else {
                    match = Regex.Match(sidekickReference.Path, @"(.+packages\\Ammy)\.\w+\\(\d+\.\d+\.\d+)");
                    if (match.Success && match.Groups.Count >= 3)
                        packagesPath = match.Groups[1].Value + "\\" + match.Groups[2].Value + "\\build";
                }
            }
            
            if (packagesPath == null) {
                var solutionDirectoryName = Path.GetDirectoryName(dte.Solution.FullName);
                if (solutionDirectoryName == null)
                        throw new DirectoryNotFoundException("Ammy extension couldn't find solution directory");
                
                var packagesFolder = Path.Combine(solutionDirectoryName, "packages");
                var comparer = new AmmyVersionComparer();
                var ammyPackagePath = Directory.GetDirectories(packagesFolder)
                                               .Where(path => Regex.IsMatch(path, @"Ammy\.\d+\.\d+\.\d+"))
                                               .OrderByDescending(path => path, comparer)
                                               .FirstOrDefault();

                if (ammyPackagePath != null)
                    packagesPath = Path.Combine(ammyPackagePath, "build");
            }

            if (packagesPath == null)
                throw new DirectoryNotFoundException("Ammy extension couldn't find packages directory." + nl + nl +
                                                     "Run `install-package Ammy` in Package Manager Console and restart Visual Studio");

            return packagesPath;

            // Can't copy files from here, because they a being loaded into VS by msbuild first
            // Need a Task that would copy contents of `Ammy.x.x.x\build` into temp directory before compilation

            //try {
            //    var tempPath = Path.GetTempPath();
            //    var packageDirectoryName = new DirectoryInfo(ammyPackagePath).Name;
            //    var runtimeDirectoryPath = Path.Combine(tempPath, packageDirectoryName);

            //    if (!Directory.Exists(runtimeDirectoryPath))
            //        Directory.CreateDirectory(runtimeDirectoryPath);

            //    var files = Directory.GetFiles(ammyPackageBuildPath);

            //    foreach (var file in files) {
            //        try {
            //            var dest = Path.GetFileName(file);
            //            File.Copy(file, Path.Combine(runtimeDirectoryPath, dest));
            //        } catch {
            //        }
            //    }

            //    return runtimeDirectoryPath;
            //} catch {
            //    return ammyPackageBuildPath;
            //}
        }

        private static void EnsureVersion(string version)
        {
            var comparer = new AmmyVersionComparer();
            // If Ammy version is lower than 1.2.21 show message box
            if (comparer.Compare("Ammy" + version, "Ammy.1.2.20") < 0 && version != ".1.0.0")
                throw new DirectoryNotFoundException("Please update Ammy NuGet package. " + Environment.NewLine + Environment.NewLine +
                                                     "Current extension is not compatible with packages older than 1.2.21");
        }

        private static Project GetProjectFromCache(string key, Func<string, Project> resolver)
        {
            Project project;
            if (ProjectCache.TryGetValue(key, out project)) {
                try {
                    Debug.WriteLine(project.Name); // Check that project is available
                    return project;
                } catch {
                    // Update cache if not available 
                    return ProjectCache[key] = resolver(key);
                }
            } else {
                return ProjectCache[key] = resolver(key);
            }
        }

        public static Project GetProjectByFilename(DTE dte, string filename, IServiceProvider serviceProvider)
        {
            return GetProjectFromCache(filename, _ => {
                if (dte != null) {
                    var projectItem = dte.Solution.FindProjectItem(filename);
                    return projectItem?.ContainingProject;
                }

                return null;
            });
        }
    }

    class AmmyVersionComparer : IComparer<string>
    {
        public int Compare(string left, string right)
        {
            try {
                var leftVersion = GetVersion(left);
                var rightVersion = GetVersion(right);

                var major = leftVersion.Item1.CompareTo(rightVersion.Item1);
                if (major != 0)
                    return major;

                var minor = leftVersion.Item2.CompareTo(rightVersion.Item2);
                if (minor != 0)
                    return minor;

                var patch = leftVersion.Item3.CompareTo(rightVersion.Item3);
                if (patch != 0)
                    return patch;

                return 0;
            } catch {
                return 0;
            }
        }

        private Tuple<int, int, int> GetVersion(string str)
        {
            var match = Regex.Match(str, @"Ammy\.(\d+)\.(\d+)\.(\d+)");

            var major = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var minor = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            var patch = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

            return Tuple.Create(major, minor, patch);
        }
    }
}
