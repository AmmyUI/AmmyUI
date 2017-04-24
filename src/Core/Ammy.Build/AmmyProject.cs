using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Nitra.ProjectSystem;
using Ammy.Infrastructure;
using Ammy.Language;
using Ammy.Platforms;
using DotNet;
using Microsoft.Build.Utilities;

namespace Ammy.Build
{
    public class AmmyProject
    {   
        public FsProject<Top> FsProject { get; }
        public CSharpProject CSharpProject { get; }
        public IReadOnlyList<AmmyFile<Top>> Files { get; }
        public List<string> MissingFiles { get; } = new List<string>();
        public IReadOnlyList<FileLibReference> References { get; private set; }
        public string OutputPath { get; }
        public string RootNamespace { get; }
        public string AssemblyName { get; }

        public AmmyDependentPropertyEvalContext Context => ProjectSupport.Context;
        public AmmyLanguage AmmyLanguage => AmmyLanguage.Instance;
        public Start ProjectSupport { get; }

        public IAmmyPlatform Platform { get; }
        public string PlatformName { get { return Platform?.Name ?? "WPF"; } }
        public string TargetPath { get; set; }

        private readonly CancellationTokenSource _cts;

        public AmmyProject(IReadOnlyList<string> referenceAssemblies, IReadOnlyList<AmmyFileMeta> ammyFiles, CSharpProject csharpProject, object compilationData, string projectDir, string outputPath, string rootNamespace, string assemblyName, string targetPath)
        {
            _cts = new CancellationTokenSource();
            CSharpProject = csharpProject;
            TargetPath = targetPath;

            References = referenceAssemblies.Distinct()
                                            .SelectMany(path => new[] { new FileLibReference(path) })
                                            .ToList();

            FsProject = new FsProject<Top>(new FsSolution<Top>(), projectDir, References) { Data = compilationData };
            Files = ammyFiles.SelectMany((meta, index) => {
                if (!System.IO.File.Exists(meta.Filename)) {
                    MissingFiles.Add(meta.Filename);
                    return new AmmyFile<Top>[0];
                }

                // If file is not cached by VS extension create new
                if (meta.File == null) {
                    meta.File = new AmmyFile<Top>(index, meta, AmmyLanguage, projectDir, FsProject);
                } else {
                    FsProject.FsFiles.Add(meta.File);
                }

                return new[] { meta.File };
            }).ToList();

            OutputPath = outputPath;
            RootNamespace = rootNamespace;
            AssemblyName = assemblyName;

            ProjectSupport = (Start)FsProject.GetProjectSupport();

            if (ProjectSupport != null) {
                Platform = ProjectSupport.Platform = GetCompatiblePlatform();
            }
        }

        private IAmmyPlatform GetCompatiblePlatform()
        {
            if (References.Any(r => r.Path.EndsWith("Xamarin.Forms.Core.dll", StringComparison.InvariantCultureIgnoreCase)))
                return new XamarinFormsPlatform();

            if (References.Any(r => r.Path.EndsWith("Avalonia.Controls.dll", StringComparison.InvariantCultureIgnoreCase)))
                return new AvaloniaPlatform();

            if (References.Any(r => r.Path.EndsWith("System.Runtime.WindowsRuntime.dll", StringComparison.InvariantCultureIgnoreCase)))
                return new UwpPlatform();

            return new WpfPlatform();
        }

        public object RefreshReferences()
        {
            if (ProjectSupport == null)
                throw new Exception("ProjectSupport not found");

            return ProjectSupport.RefreshReferences(_cts.Token, FsProject);
        }

        public void RefreshProject(string outputPath, string rootNamespace, string assemblyName)
        {
            if (ProjectSupport == null)
                throw new Exception("ProjectSupport not found");

            var files = Files.SelectMany(f => new[] { f.EvalPropertiesData })
                             .ToImmutableArray();
            
            ProjectSupport.Context.RootNamespace = rootNamespace;
            ProjectSupport.Context.AssemblyName = assemblyName;
            ProjectSupport.Context.OutputPath = outputPath;
            ProjectSupport.RefreshProject(_cts.Token, files, FsProject.Data);
        }
    }
}