using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Ammy.Infrastructure;
using Nitra;

namespace Ammy.Build
{
    public class AmmyCompiler
    {
        private readonly bool _isMsBuildCompilation;

        public AmmyCompiler(bool isMsBuildCompilation = false)
        {
            _isMsBuildCompilation = isMsBuildCompilation;
        }

        public CompileResult Compile(string rootNamespace, string[] sourceFiles, IReadOnlyList<string> referenceAssemblies, string projectDir, string outputPath, string assemblyName, string targetPath, object compilationData, bool generateMetaFile = false, bool needBamlGeneration = false, bool needUpdate = true)
        {
            var ammyFiles = sourceFiles.Where(path => Path.GetExtension(path) == ".ammy")
                                       .SelectMany(path => new[] { CreateAmmyFileMeta(path, projectDir) })
                                       .ToList();

            var allSourceFiles = sourceFiles.ToArray();

            return Compile(new AmmyProject(referenceAssemblies, ammyFiles, new CSharpProject(projectDir, allSourceFiles), compilationData, projectDir, outputPath, rootNamespace, assemblyName, targetPath), generateMetaFile, needBamlGeneration, needUpdate);
        }

        public CompileResult Compile(AmmyProject project, bool generateMetaFile = false, bool needBamlGeneration = false, bool needUpdate = true)
        {
            var compileResult = new CompileResult(needBamlGeneration) {
                AmmyProject = project
            };

            if (project.ProjectSupport == null)
                return compileResult;

            try {
                if (project.MissingFiles.Count > 0) {
                    foreach (var missingFile in project.MissingFiles)
                        compileResult.AddError(missingFile + " does not exist.");

                    return compileResult;
                }

                foreach (var file in project.Files)
                    compileResult.Files.Add(file);

                compileResult.CompilationData = project.RefreshReferences();

                project.Context.ProjectDir = project.FsProject.ProjectDir;
                project.Context.SourceCodeProject = project.CSharpProject;
                project.Context.NeedUpdate = project.Platform.SupportsRuntimeUpdate && needUpdate;
                
                project.RefreshProject(project.OutputPath, project.RootNamespace, project.AssemblyName);

                EnsureOutputDirectoryExists(project.OutputPath.ToAbsolutePath(project.FsProject.ProjectDir));
                
                var anyErrors = compileResult.CompilerMessages.Any(msg => msg.Type == CompilerMessageType.Error || msg.Type == CompilerMessageType.FatalError);
                var fileOutputWriter = new FileOutputWriter(compileResult, project.Context, project.Files, project.FsProject.ProjectDir, project.OutputPath);
                var bamlCompiler = new BamlCompiler(_isMsBuildCompilation);

                if (!anyErrors) {
                    fileOutputWriter.WriteFiles(generateMetaFile);

                    if (needBamlGeneration)
                        bamlCompiler.CompileBamlFiles(compileResult, project);
                }
            } catch (Exception e) {
                compileResult.AddError("Ammy compilation error: " + e);
            }

            return compileResult;
        }

        private static void EnsureOutputDirectoryExists(string outputPath)
        {
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
        }

        private AmmyFileMeta CreateAmmyFileMeta(string path, string projectDir)
        {
            return new AmmyFileMeta(path.ToAbsolutePath(projectDir));
        }
    }
}