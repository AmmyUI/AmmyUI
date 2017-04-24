using System.Collections.Concurrent;
using System.Collections.Generic;
using EnvDTE;
using Microsoft.VisualStudio.Text;

namespace Ammy.VisualStudio.Service.Compilation
{
    public class ShadowCopyStorage
    {
        private readonly ConcurrentDictionary<string, ShadowCopyProject> _projects = new ConcurrentDictionary<string, ShadowCopyProject>();

        private readonly object _lock = new object();
        
        public ShadowCopyProject ResolveShadowCopyProject(List<string> filenames, Project project)
        {
            var shadowCopy = GetProject(project);
            shadowCopy.UpdateFiles(filenames);
            return shadowCopy;
        }

        public void Commit(Project project)
        {
            lock (_lock) {
                var shadowCopy = GetProject(project);
                shadowCopy.Commit();
            }
        }

        public void BufferCreated(ITextBuffer buffer, ITextDocument document, Project project)
        {
            if (document == null || project == null)
                return;

            var shadowCopy = GetProject(project);
            var projectItem = shadowCopy.GetOrAddProjectItem(document.FilePath);

            projectItem.Buffer = buffer;
        }

        public void BufferDisposed(string filename, Project project)
        {
            var projectItem = GetProjectItem(project, filename);
            projectItem.Buffer = null;
        }

        public string GetTemporaryFilename(Project project, string originalFilename)
        {
            return GetProjectItem(project, originalFilename)?.TemporaryFilename;
        }

        public ShadowCopyProjectItem GetProjectItem(Project project, string filePath)
        {
            var shadowCopy = _projects.GetOrAdd(project.Name, _ => new ShadowCopyProject(project.Name, project.FileName));
            return shadowCopy.GetOrAddProjectItem(filePath);
        }

        public ShadowCopyProject GetProject(Project project)
        {
            return _projects.GetOrAdd(project.Name, _ => new ShadowCopyProject(project.Name, project.FileName));
        }
    }
}