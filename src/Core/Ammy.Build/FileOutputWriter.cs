using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using DotNet;
using Ammy.Backend;
using Ammy.Infrastructure;
using Ammy.Language;
using Ammy.Platforms;
using Ammy.Xaml;

namespace Ammy.Build
{
    public class FileOutputWriter
    {
        private readonly CompileResult _compileResult;
        private readonly AmmyDependentPropertyEvalContext _context;
        private readonly IReadOnlyList<AmmyFile<Top>> _files;
        private readonly string _projectPath;
        private readonly string _outputPath;

        public FileOutputWriter(CompileResult compileResult)
        {
            var ammyProject = compileResult.AmmyProject;

            _compileResult = compileResult;
            _files = compileResult.Files.ToList();
            _context = ammyProject.Context;
            _projectPath = ammyProject.FsProject.ProjectDir;
            _outputPath = ammyProject.OutputPath;
        }

        public void WriteFiles(bool generateMetaFile)
        {
            var fullOutputPath = Path.Combine(_projectPath, _outputPath);
            var projectMeta = _compileResult.ProjectMeta;

            if (!Directory.Exists(fullOutputPath))
                Directory.CreateDirectory(fullOutputPath);

            GenerateTopNodes(projectMeta);

            if (generateMetaFile)
                GenerateMetaFile(fullOutputPath, projectMeta);
        }

        private void GenerateTopNodes(XamlProjectMeta projectMeta)
        {
            foreach (var file in _files) {
                var topWithNode = ((Start) file.Ast).Top as TopWithNode;

                if (topWithNode == null)
                    continue;

                if (!topWithNode.IsXamlEvaluated)
                    throw new Exception("Xaml node is not evaluated in file " + file.Meta.Filename);

                var functionRefs = topWithNode.TopNode.FunctionRefScope.GetSymbols();
                var rootFunctionRefs = functionRefs.SelectMany(fr => new[] { fr.FirstDeclarationOrDefault })
                                                   .OfType<ContentFunctionRef>()
                                                   .Where(cfr => cfr.IsParentEvaluated && cfr.Parent == topWithNode.TopNode)
                                                   .SelectMany(cfr => new[] { cfr.FunctionRef.Symbol.FirstDeclarationOrDefault })
                                                   .OfType<GlobalDeclaration.ContentFunction>()
                                                   .SelectMany(cf => cf.Members);

                var xamlNode = (XamlNode) topWithNode.Xaml;
                var xaml = xamlNode.Build();
                var xamlHash = MD5.Create().ComputeHash(Encoding.Unicode.GetBytes(xaml));
                var propertyMetas = GetPropertyMetas(topWithNode.TopNode.Members);
                var propertyMetasFromFunctions = GetPropertyMetas(rootFunctionRefs);

                // Generate imlementation
                var ammyProject = _compileResult.AmmyProject;

                var outputFileSuffix = ammyProject.Platform.OutputFileSuffix;
                var xamlFilePath = Path.ChangeExtension(file.OutputFilename, outputFileSuffix + ".xaml");


                for (int i = 0; i < 3; i++) {
                    try {
                        File.WriteAllText(xamlFilePath, xaml);
                        break;
                    } catch (IOException) {
                        // Might throw access denied exception, so retry few times
                        Thread.Sleep(50);
                    }
                }

                var metaFilePath = xamlFilePath;

                if (ammyProject.Platform is WpfPlatform) {
                    var projectDir = ammyProject.FsProject.ProjectDir;
                    var objDir = Path.Combine(projectDir, ammyProject.OutputPath);
                    var relativeFile = file.OutputFilename.ToRelativeFile(projectDir);
                    var bamlRelativeFile = Path.ChangeExtension(relativeFile, outputFileSuffix + ".baml");

                    metaFilePath = Path.Combine(objDir, bamlRelativeFile);
                }

                projectMeta.Files.Add(new XamlFileMeta {
                    FilePath = metaFilePath,
                    Filename = xamlFilePath.ToRelativeFile(_projectPath),
                    Hash = BitConverter.ToString(xamlHash),
                    Properties = propertyMetas.Concat(propertyMetasFromFunctions)
                                              .ToList()
                });

                _compileResult.GeneratedXamlFiles.Add(xamlFilePath);
            }
        }

        private static void GenerateMetaFile(string fullOutputPath, XamlProjectMeta projectMeta)
        {
            var metaFilename = Path.Combine(fullOutputPath, "ammy.meta");

            using (var writer = new StreamWriter(metaFilename, false)) {
                var serializer = new XmlSerializer(typeof (XamlProjectMeta));
                serializer.Serialize(writer, projectMeta);
            }
        }

        private static IEnumerable<XamlPropertyMeta> GetPropertyMetas(IEnumerable<NodeMember> members)
        {
            return members.OfType<Property>()
                          .SelectMany(prop => {
                              var symbol = prop.Ref.Symbol;

                              PropertyType type;
                              if (symbol is DependencyPropertySymbol) {
                                  type = PropertyType.DependencyProperty;
                              } else if (symbol is RoutedEventSymbol) {
                                  type = PropertyType.RoutedEvent;
                              } else if (symbol is Member.EventSymbol) {
                                  type = PropertyType.Event;
                              } else {
                                  type = PropertyType.Property;
                              }

                              return new[] { new XamlPropertyMeta {
                                  PropertyType = type,
                                  FullName = symbol.FullName
                              } };
                          });
        }
    }
}