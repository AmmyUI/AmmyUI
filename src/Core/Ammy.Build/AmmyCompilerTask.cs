using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Nitra;
using Ammy.Infrastructure;

namespace Ammy.Build
{
    public class AmmyCompilerTask : Task
    {
        [Required]
        public string Items { get; set; }

        [Required]
        public string SourceFiles { get; set; }
        
        public string IncludedPages { get; set; }

        [Required]
        public string References { get; set; }

        [Required]
        public string OutputPath { get; set; }

        [Required]
        public string RootNamespace { get; set; }

        [Required]
        public string AssemblyName { get; set; }

        [Required]
        public string ProjectDir { get; set; }

        [Required]
        public string TargetPath { get; set; }

        [Required]
        public string DefineConstants { get; set; }

        [Output]
        public string[] GeneratedItems { get; set; }

        [Output]
        public string[] GeneratedXamlItems { get; set; }

        [Output]
        public bool HasErrors { get; set; }

        [Output]
        public string AmmyPlatform { get; set; }

        public override bool Execute()
        {
            //Debugger.Launch();

            // Don't rebuild 20 times when updating/installing from NuGet or adding item template
            if (Environment.StackTrace.Contains("at NuGet.PackageManagement.VisualStudio") ||
                Environment.StackTrace.Contains("at System.Windows.Controls.PopupControlService") ||
                Environment.StackTrace.Contains("at Microsoft.VisualStudio.Build.ComInteropWrapper.ProjectShim.BuildTargetsImpl") ||
                Environment.StackTrace.Contains("at Microsoft.VisualStudio.TemplateWizard.Wizard")) {
                GeneratedItems = new string[0];
                GeneratedXamlItems = new string[0];

                HasErrors = true;
                AmmyPlatform = "<notset>";

                return true;
            }

            var allSources = Items.Split(';')
                                  .Concat(SourceFiles.Split(';'))
                                  .ToArray();

            var references = References.Split(';')
                                       .ToList();

            var pages = IncludedPages ?? "";
            var includedPages = pages.Split(';')
                                     .ToList();
            //var currentAssemblyPath = Path.Combine(ProjectDir, OutputPath, AssemblyName + ".exe");

            //if (File.Exists(currentAssemblyPath))
            //    references.Add(currentAssemblyPath);

            if (Items.Length == 0)
                return true;

            var constants = DefineConstants?.Split(';');
            var needUpdate = true;

            if (constants != null) {
                var isDebug = constants.Contains("DEBUG");
                var noAmmyUpdate = constants.Contains("NO_AMMY_UPDATE");
                needUpdate = isDebug && !noAmmyUpdate;
            }

            try {
                var compiler = new AmmyCompiler(isMsBuildCompilation: true);
                var result = compiler.Compile(RootNamespace, allSources, references, ProjectDir, OutputPath, AssemblyName, TargetPath, null, true, true, needUpdate);

                Log.LogMessage("Ammy update enabled: " + needUpdate);

                foreach (var msg in result.CompilerMessages) {
                    if (msg.Type == CompilerMessageType.Error || msg.Type == CompilerMessageType.FatalError) {
                        var loc = msg.Location;
                        var start = loc.StartLineColumn;
                        var end = loc.EndLineColumn;
                        Log.LogError("", "", "", msg.Location.Source.File.FullName, start.Line, start.Column, end.Line, end.Column, msg.Text);
                    } else if (msg.Type == CompilerMessageType.Warning) {
                        Log.LogWarning(msg.ToString());
                    }
                }

                GeneratedItems = result.GeneratedFiles.ToArray();
                GeneratedXamlItems = result.GeneratedXamlFiles
                                           .SelectMany(filename => new[] { filename.ToRelativeFile(ProjectDir) })
                                           .Where(fname => !ListContainsFile(fname, includedPages))
                                           .ToArray();
                
                HasErrors = !result.IsSuccess;
                AmmyPlatform = result.AmmyProject.PlatformName;

                return true;
            }
            catch (Exception e) {
                Log.LogError(e.Message);

                HasErrors = true;

                return false;
            }
        }

        private bool ListContainsFile(string file, List<string> fileList)
        {
            foreach (var fileListFile in fileList)
                if (file.SamePathAs(fileListFile))
                    return true;

            return false;
        }
    }
}