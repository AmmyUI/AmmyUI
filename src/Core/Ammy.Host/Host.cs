using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Ammy.Platforms;
using Ammy.Language;
using Nitra.Declarations;
using Nitra.ProjectSystem;
using CompilerMessage = Ammy.Platforms.CompilerMessage;
using CompilerMessageType = Ammy.Platforms.CompilerMessageType;
using Type = System.Type;

// ReSharper disable once CheckNamespace
namespace Ammy
{
    public class Host
    {
        private readonly IAmmyPlatform _platform;
        private Type[] _typeCache;

        public Host(IAmmyPlatform platform)
        {
            _platform = platform;
        }

        public CompilationResult Compile(CompilationRequest request)
        {
            if (request.NeedTypeReloading || _typeCache == null)
                _typeCache = _platform.ProvideTypes();

            var files = request.Sources.OfType<FileSource>()
                .Select((fs, index) => new InputFile(index, fs.Path, AmmyLanguage.Instance))
                .ToArray();

            if (files.Length > 0) {
                var cts = new CancellationTokenSource();
                var references = request.ReferenceAssemblyPaths.Select(path => new FileLibReference(path));
                var project = new FsProject<Start>(new FsSolution<Start>(), request.ProjectDir, files, references) {
                    Data = request.Data
                };

                var projectSupport = files.Select(f => f.Ast)
                    .OfType<IProjectSupport>()
                    .FirstOrDefault();

                if (projectSupport == null)
                    throw new Exception("IProjectSupport not found");

                if (!(projectSupport is Start))
                    throw new Exception("IProjectSupport should have AST type Start");

                var start = (Start)projectSupport;

                start.Platform = _platform;

                var data = projectSupport.RefreshReferences(cts.Token, project);

                var fileEvals = files.Select(fs => fs.GetEvalPropertiesData())
                    .ToImmutableArray();

                projectSupport.RefreshProject(cts.Token, fileEvals, request.Data ?? data);

                var compilerMessages = fileEvals.SelectMany(f => f.GetCompilerMessage())
                    .Select(ToBackendCompilerMessage)
                    .ToArray();

                var hasError = compilerMessages.FirstOrDefault(msg => msg.Type == CompilerMessageType.Error) != null;
                var outputFiles = files.SelectMany(ToOutputFile).ToArray();

                return new CompilationResult("", !hasError, compilerMessages, outputFiles);
            }

            return new CompilationResult("", true, new CompilerMessage[0], new OutputFile[0]);
        }

        private IEnumerable<OutputFile> ToOutputFile(FsFile<Start> fsFile)
        {
            var ast = (Start)fsFile.Ast;

            if (ast.IsAllPropertiesEvaluated && ast.Top is TopWithNode) {
                var withNode = (TopWithNode)ast.Top;
                if (withNode.IsXamlEvaluated) {
                    yield return new OutputFile(fsFile.FullName, withNode.Xaml.Build());
                }
            }
        }

        private CompilerMessage ToBackendCompilerMessage(Nitra.ProjectSystem.CompilerMessage arg)
        {
            CompilerMessageType type;
            if (arg.Type == Nitra.CompilerMessageType.Error || arg.Type == Nitra.CompilerMessageType.FatalError)
                type = CompilerMessageType.Error;
            else if (arg.Type == Nitra.CompilerMessageType.Warning)
                type = CompilerMessageType.Warning;
            else
                type = CompilerMessageType.Hint;

            var start = arg.Location.StartLineColumn;

            return new CompilerMessage(type, arg.Text, new CompilerMessageLocation(start.Line, start.Column, arg.Location.Source.File.FullName));
        }
    }
}
