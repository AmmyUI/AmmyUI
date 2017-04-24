using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ammy.VisualStudio.Service.Compilation
{
    public class ShadowCopyProject : INeedLogging
    {
        private readonly string _originalProjectPath;

        public ICollection<ShadowCopyProjectItem> Items => _items.Values;
        public string ProjectPath { get; set; }
        public string ProjectName { get; set; }

        private readonly ConcurrentDictionary<string, ShadowCopyProjectItem> _items = new ConcurrentDictionary<string, ShadowCopyProjectItem>();
        
        public ShadowCopyProject(string projectName, string projectFilePath)
        {
            _originalProjectPath = Path.GetDirectoryName(projectFilePath);

            ProjectName = projectName;
            ProjectPath = Path.Combine(Path.GetTempPath(), projectName + "_Shadow");

            try {
                if (Directory.Exists(ProjectPath))
                    Directory.Delete(ProjectPath, true);

                Directory.CreateDirectory(ProjectPath);
            } catch (Exception e) {
                this.LogDebugInfo("Shadow copy directory exception: " + e);
            }
        }

        public void UpdateFiles(List<string> filenames)
        {
            foreach (var filename in filenames)
                GetOrAddProjectItem(filename);

            var filesToRemove = new List<string>();

            foreach (var projectItem in _items.Values)
                if (filenames.All(fn => fn != projectItem.OriginalFilename))
                    filesToRemove.Add(projectItem.OriginalFilename);

            foreach (var fileToRemove in filesToRemove) {
                ShadowCopyProjectItem item;
                _items.TryRemove(fileToRemove, out item);
            }
        }

        public ShadowCopyProjectItem GetOrAddProjectItem(string originalFilename)
        {
            return _items.GetOrAdd(originalFilename, _ => {
                var temporaryFilename = originalFilename.Replace(_originalProjectPath, ProjectPath);

                if (temporaryFilename == originalFilename) {
                    temporaryFilename = Path.GetFileName(originalFilename);
                    temporaryFilename = Path.Combine(ProjectPath, temporaryFilename);
                }

                return new ShadowCopyProjectItem(temporaryFilename, originalFilename);
            });
        }
        
        public void Commit()
        {
            foreach (var item in Items) {
                var fileName = item.TemporaryFilename;

                EnsureFileExists(fileName);

                if (item.Buffer != null)
                    File.WriteAllText(fileName, item.Buffer.CurrentSnapshot.GetText());
                else
                    File.Copy(item.OriginalFilename, fileName, true);
            }
        }

        private static void EnsureFileExists(string fileName)
        {
            if (!File.Exists(fileName)) {
                var directory = Path.GetDirectoryName(fileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(fileName, "");
            }
        }
    }
}