using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks.Windows;
using Microsoft.Build.Utilities;

namespace Ammy.BamlCompilerWPF
{
    class Program
    {
        static void Main(string[] args)
        {
            var references = new List<string>();
            var sources = new List<string>();
            var markup = new List<string>();

            string assemblyName = null;
            string outputPath = null;
            string targetPath = null;

            string rootNamespace = null;
            var argsFilename = args[0];
            var argsText = File.ReadAllText(argsFilename);
            var argsFromFile = CommandLine.ToArgs(argsText);

            try {
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "ammy-baml.log"), argsText);
            } catch (Exception e) {
                Debug.WriteLine("Unable to save baml log: " + e);
            }

            foreach (var arg in argsFromFile) {
                var split = arg.Split(new[] {':'}, 2);

                if (split.Length == 2) {
                    if (split[0] == "/targetPath")
                        targetPath = split[1];

                    if (split[0] == "/markup")
                        markup.AddRange(split[1].Split(','));

                    if (split[0] == "/references")
                        references.AddRange(split[1].Split(','));

                    if (split[0] == "/assemblyName")
                        assemblyName = split[1];

                    if (split[0] == "/outputPath")
                        outputPath = split[1];

                    if (split[0] == "/sources")
                        sources.AddRange(split[1].Split(','));

                    if (split[0] == "/rootNamespace")
                        rootNamespace = split[1];
                }
            }

            //Debugger.Launch();

            if (assemblyName == null || rootNamespace == null || outputPath == null || references.Count == 0 || markup.Count == 0) {
                Console.Error.WriteLine("Not all of required parameters supplied.\r\nFormat: bamlcwpf.exe /assemblyName=A /rootNamespace=B /outputPath=obj/Debug /markup=a.xaml,b.xaml /references=a.dll,b.dll");
                return;
            }

            Compile(assemblyName, rootNamespace, outputPath, targetPath, references, markup, sources);
        }
        
        private static void Compile(string assemblyName, string rootNamespace, string outputPath, string targetPath, List<string> references, List<string> markup, List<string> sources)
        {
            var referenceItems = references.Select(r => new TaskItem(r) as ITaskItem)
                                           .ToArray();
            var markupItems = markup.Select(r => new TaskItem(r) as ITaskItem).ToArray();
            var buildEngine = new Engine();
            var sourceFileItems = sources.Select(src => new TaskItem(src) as ITaskItem).ToArray();

            var markupCompilePass1 = new MarkupCompilePass1 {
                BuildEngine = buildEngine,
                Language = "C#",
                SourceCodeFiles = sourceFileItems,
                //ApplicationMarkup = new ITaskItem[] {
                //   new TaskItem(@"c:\Users\Mihhail\AppData\Local\Temp\csharp-wpf-test-project\App.xaml"),
                //},
                PageMarkup = markupItems,
                AssemblyName = assemblyName,
                LanguageSourceExtension = ".cs",
                AlwaysCompileMarkupFilesInSeparateDomain = true,
                XamlDebuggingInformation = true,
                OutputPath = outputPath,
                OutputType = "Exe",
                HostInBrowser = "false",
                RootNamespace = rootNamespace,
                References = referenceItems
            };

            var result = markupCompilePass1.Execute();
//            var temporaryAssembly = Path.Combine(outputPath, assemblyName + ".exe");
            var temporaryAssembly = targetPath;
            
            var markupCompilePass2 = new MarkupCompilePass2 {
                BuildEngine = buildEngine,
                AssemblyName = assemblyName,
                OutputType = "Exe",
                Language = "C#",
                LocalizationDirectivesToLocFile = "",
                RootNamespace = rootNamespace,
                References = referenceItems.Concat(new[] { new TaskItem(temporaryAssembly) })
                                           .ToArray(),
                AlwaysCompileMarkupFilesInSeparateDomain = true,
                XamlDebuggingInformation = true,
                OutputPath = outputPath,
            };

            markupCompilePass2.BuildEngine = markupCompilePass1.BuildEngine;
            result = markupCompilePass2.Execute();

            Console.WriteLine("Pass 1 generated");
            foreach (var generatedBamlFile in markupCompilePass1.GeneratedBamlFiles)
                Console.WriteLine("_o|" + generatedBamlFile.ItemSpec);

            Console.WriteLine("Pass 2 generated");
            foreach (var generatedBamlFile in markupCompilePass2.GeneratedBaml)
                Console.WriteLine("_o|" + generatedBamlFile.ItemSpec);
        }
    }
}
