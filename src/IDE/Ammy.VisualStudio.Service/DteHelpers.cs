using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Ammy.VisualStudio.Service
{
    public static class DteHelpers
    {
        private static readonly ConcurrentDictionary<string, Project> ProjectCache = new ConcurrentDictionary<string, Project>();
         
        public static ProjectItem GetProjectItemFromFilename(string filename)
        {
            try {
                var dte = (DTE) Package.GetGlobalService(typeof (SDTE));
                return dte.Solution.Projects.OfType<Project>()
                          .SelectMany(p => GetFileList(p))
                          .FirstOrDefault(pi => GetFilename(pi).Equals(filename, StringComparison.InvariantCultureIgnoreCase));
            } catch {
                return null;
            }
        }
        
        public static ProjectItem[] GetFileList(Project genericProject)
        {
            return GetProjectItems(genericProject.ProjectItems).ToArray();
        }

        public static IEnumerable<ProjectItem> GetProjectItems(ProjectItems root)
        {
            foreach (var pi in root) {
                var projectItem = (ProjectItem)pi;
                var children = projectItem.ProjectItems;

                if (children != null)
                    foreach (var childItem in GetProjectItems(children))
                        yield return childItem;

                yield return projectItem;
            }
        }

        public static string GetFilename(ProjectItem pi)
        {
            try {
                return pi.FileNames[0];
            } catch (Exception) {
                return "";
            }
        }

        public static Project GetProjectByFilename(string filename)
        {
            return GetProjectFromCache(filename, _ => {
                var dte = (DTE) Package.GetGlobalService(typeof (SDTE));

                if (dte != null) {
                    var projectItem = dte.Solution.FindProjectItem(filename);
                    return projectItem?.ContainingProject;
                }

                return null;
            });
        }

        public static Project GetProjectByName(string projectName)
        {
            return GetProjectFromCache(projectName, _ => {
                var dte = (DTE)Package.GetGlobalService(typeof(SDTE));

                if (dte != null) {
                    foreach (var p in dte.Solution.Projects) {
                        var project = (Project)p;
                        if (project.Name.Equals(projectName, StringComparison.InvariantCultureIgnoreCase))
                            return project;
                    }
                }

                return null;
            });
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
    }
}