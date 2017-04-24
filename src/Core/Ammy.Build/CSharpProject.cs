using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ammy.Backend;
using Ammy.Infrastructure;
using CSharp;
using Nitra.ProjectSystem;
using File = System.IO.File;

namespace Ammy.Build
{
    public class CSharpProject : ISourceCodeProject
    {
        private readonly string _projectDir;

        public Nitra.ProjectSystem.File[] Files { get; private set; }
        public HashSet<int> DirtyFiles { get; private set; }
        public bool IsParsed { get; set; }

        public CSharpProject(string projectDir, string[] filenames)
        {
            _projectDir = projectDir;
            DirtyFiles = new HashSet<int>();
            Reload(filenames);
        }

        public string[] GetFilenames()
        {
            return Files.OfType<CSharpFile>()
                        .SelectMany(f => new[] { f.FullName })
                        .ToArray();
        }

        public void MarkAsDirty(string filename)
        {
            foreach (var file in Files) {
                if (file.FullName.Equals(filename, StringComparison.InvariantCultureIgnoreCase)) {
                    DirtyFiles.Add(file.Id);
                    break;
                }
            }
        }

        public void ClearDirty()
        {
            DirtyFiles.Clear();
        }

        private void Reload(string[] filenames)
        {
            IsParsed = false;
            var files = new List<CSharpFile>();

            for (int index = 0; index < filenames.Length; index++) {
                var filename = filenames[index];
                var extension = Path.GetExtension(filename);

                if (extension != ".cs" || filename.EndsWith(".ammy.cs"))
                    continue;

                if (!Path.IsPathRooted(filename))
                    filename = filename.ToAbsolutePath(_projectDir);

                if (!File.Exists(filename))
                    continue;
                
                // 10000 is CSharp file offset
                var file = new CSharpFile(index + 10000, filename, NitraCSharp.Instance);

                files.Add(file);
            }

            Files = files.Cast<Nitra.ProjectSystem.File>().ToArray();
        }

        public class CSharpFile : FsFile<CompilationUnit>
        {
            public override int Id { get; }

            public CSharpFile(int id, string filePath, Nitra.Language language, FsProject<CompilationUnit> fsProject = null, FileStatistics statistics = null) : base(filePath, language, fsProject, statistics)
            {
                Id = id;
            }
        }
    }
}