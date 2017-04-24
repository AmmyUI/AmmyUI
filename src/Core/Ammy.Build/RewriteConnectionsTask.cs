using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Nitra;
using Ammy.Infrastructure;

namespace Ammy.Build
{
    public class RewriteConnectionsTask : Task
    {
        [Required]
        public string Items { get; set; }

        [Required]
        public string OutputPath { get; set; }

        [Required]
        public string ProjectDir { get; set; }

        [Required]
        public string DefineConstants { get; set; }

        public override bool Execute()
        {
            try {
                //return true;
                var allSourceFiles = Items.Split(';')
                                          .Where(path => Path.GetExtension(path) == ".ammy")
                                          .ToList();

                if (allSourceFiles.Count == 0)
                    return true;

                var constants = DefineConstants?.Split(';');

                if (constants != null) {
                    var isDebug = constants.Contains("DEBUG");
                    var noAmmyUpdate = constants.Contains("NO_AMMY_UPDATE");
                    if (!isDebug || noAmmyUpdate)
                        return true;
                }

                foreach (var file in allSourceFiles) {
                    var relativePath = file.ToRelativeFile(ProjectDir);
                    var csharpFile = Path.ChangeExtension(relativePath, ".g.g.cs");
                    var objDirectory = Path.Combine(ProjectDir, OutputPath);
                    var fullPath = csharpFile.ToAbsolutePath(objDirectory);

                    if (File.Exists(fullPath))
                        RewriteConnections(fullPath);
                }

                return true;
            } catch (Exception e) {
                Log.LogError(e.Message);

                return false;
            }
        }

        private void RewriteConnections(string fullPath)
        {
            try {
                var lines = File.ReadAllLines(fullPath);
                var result = new List<string>();
                var foundConnector = false;
                var connectorIndent = "";
                var rewritingFinished = false;
                var skipIsLoaded = false;

                foreach (var line in lines) {
                    result.Add(line);

                    if (line.Contains("System.Windows.Application, System.Windows.Markup.IComponentConnector") || line.Contains("System.Windows.ResourceDictionary, System.Windows.Markup.IComponentConnector"))
                        skipIsLoaded = true;

                    if (rewritingFinished)
                        continue;

                    if (foundConnector && line.StartsWith(connectorIndent + "}")) {
                        //result.Add(connectorIndent + "private int _alreadyConnected;");
                        rewritingFinished = true;
                    } else if (line.Contains("void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {")) {
                        foundConnector = true;
                        connectorIndent = string.Join("", line.TakeWhile(c => c == ' ' || c == '\t'));
                        //result.Add(connectorIndent + "    if (System.Windows.Application.Current.MainWindow != null && _alreadyConnected) return; else _alreadyConnected = true;");
                        if (!skipIsLoaded) {
                            //result.Add(connectorIndent + "    if (_alreadyConnected++ > 0) return;");
                            result.Add(connectorIndent + "    if (IsLoaded) return;");
                        }
                    }
                }

                File.WriteAllLines(fullPath, result);
            } catch (Exception e) {
                Log.LogWarning(e.Message);
            }
        }
    }
}