using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Nitra;
using Nitra.ProjectSystem;
using Ammy.Infrastructure;
using Ammy.Language;
using Ammy.Xaml;
using File = System.IO.File;

namespace Ammy.Build
{
    public class BamlCompiler
    {
        private readonly bool _isMsBuildCompilation;

        public BamlCompiler(bool isMsBuildCompilation)
        {
            _isMsBuildCompilation = isMsBuildCompilation;
        }

        public void CompileBamlFiles(CompileResult result, AmmyProject project)
        {
            var directory = Path.GetDirectoryName(GetType().Assembly.Location);
            var markup = string.Join(",", result.GeneratedXamlFiles.SelectMany(f => new[] { "\"" + f + "\"" }));

            var references = string.Join(",", project.References.SelectMany(r => new[] { "\"" + r.Path + "\"" }));
            var outputPath = project.OutputPath.TrimEnd('\\');
            var assemblyName = project.AssemblyName;
            var rootNamespace = project.RootNamespace;
            var projectDir = project.FsProject.ProjectDir;
            var sources = string.Join(",", project.CSharpProject.GetFilenames());
            var targetPath = project.TargetPath;
            var args = $"/targetPath:{targetPath} /assemblyName:{assemblyName} /rootNamespace:{rootNamespace} /outputPath:\"{outputPath}\" /markup:{markup} /references:{references} /sources:{sources}";
            var tempFileName = Path.GetTempFileName();

            File.WriteAllText(tempFileName, args);

            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = Path.Combine(directory, "bamlcwpf.exe"),
                    Arguments = tempFileName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = projectDir
                }
            };

            process.Start();

            ParseOutput(result, projectDir, process);

            process.WaitForExit();

            try {
                File.Delete(tempFileName);
            } catch { }
        }

        private void ParseOutput(CompileResult result, string projectDir, Process process)
        {
            while (!process.StandardOutput.EndOfStream) {
                var line = process.StandardOutput.ReadLine();

                if (line == null)
                    continue;

                var bamlPrefix = "_o|";
                var errorSuffix = "|_e";

                Debug.WriteLine(line);

                if (line.StartsWith(bamlPrefix)) {
                    var generatedBamlAbsoluteFilename = line.Substring(bamlPrefix.Length);
                    result.GeneratedBamlFiles.Add(generatedBamlAbsoluteFilename);
                } else if (line.EndsWith(errorSuffix)) {
                    var split = line.Split(new[] {'|'}, 4);

                    if (split.Length < 4)
                        continue;

                    var filenameAndPosition = split[0];
                    var match = Regex.Match(filenameAndPosition, @"\((\d+),(\d+),(\d+),(\d+)\)");
                    var groups = match.Groups;

                    if (match.Success && groups.Count == 5) {
                        try {
                            var row = int.Parse(groups[1].Value);
                            var column = int.Parse(groups[2].Value);
                            //var endRow = int.Parse(groups[3].Value);
                            //var endColumn = int.Parse(groups[4].Value);
                            var filenameWithoutPosition = filenameAndPosition.Substring(0, filenameAndPosition.IndexOf('('));
                            var filename = Regex.Replace(filenameWithoutPosition, @"\.i\.xaml$", ".ammy");
                            //var filename = Path.ChangeExtension(filenameWithoutPosition, ".ammy");
                            var message = split[2];

                            var positionMatch = Regex.Match(message, @"Line \d+ Position \d+");
                            if (positionMatch.Success)
                                message = message.Substring(0, positionMatch.Index);
                            
                            var outputFileSuffix = result.AmmyProject.Platform.OutputFileSuffix;
                            var file = result.Files.FirstOrDefault(f => f.FullName
                                                                         .ToAbsolutePath(projectDir)
                                                                         .ChangeExtension(outputFileSuffix + ".xaml")
                                                                         .SamePathAs(filename));

                            if (file != null) {
                                var topWithNode = ((Start) file.Ast).Top as TopWithNode;
                                if (topWithNode == null || topWithNode.IsXamlEvaluated == false)
                                    continue;

                                var xamlElement = FindXamlElement(topWithNode.Xaml, row, column);
                                if (xamlElement != null) {
                                    // Some errors are resolved after fuul initial compilation
                                    // Don't want to stop MSBuild because of baml errors
                                    var msgType = _isMsBuildCompilation
                                        ? CompilerMessageType.Warning
                                        : CompilerMessageType.Error;
                                    var msg = new CompilerMessage(msgType, Guid.Empty, xamlElement.OriginalLocation, message, 0, new List<CompilerMessage>());

                                    var errorFile = msg.Location.Source?.File as AmmyFile<Top>;
                                    if (errorFile != null)
                                        errorFile.BamlCompilerMessages.Add(msg);
                                    else
                                        file.BamlCompilerMessages.Add(msg);
                                }
                            }
                        } catch (Exception e) {
                            Debug.WriteLine("Failed parsing '_e:' line: " + line + Environment.NewLine + e);
                        }
                    }
                }
            }

            var err = process.StandardError.ReadToEnd();
            //Debug.WriteLine(err);
        }

        private XamlElement FindXamlElement(XamlElement parent, int row, int column)
        {
            var start = parent.Start;
            var end = parent.End;
            var startRow = start.Row + 1;
            var endRow = end.Row + 1;

            if (startRow <= row && start.Column <= column &&
                endRow >= row && end.Column >= column) {

                var node = parent as XamlNode;
                if (node != null) {
                    foreach (var childNode in node.ChildNodes) {
                        var childResult = FindXamlElement(childNode, row, column);

                        if (childResult != null)
                            return childResult;
                    }

                    if (node.Value != null) {
                        var valueResult = FindXamlElement(node.Value, row, column);

                        if (valueResult != null)
                            return valueResult;
                    }

                    foreach (var attribute in node.Attributes) {
                        var attributeResult = FindXamlElement(attribute, row, column);

                        if (attributeResult != null)
                            return attributeResult;
                    }

                    return node;
                }

                return parent;
            }

            return null;
        }
    }
}