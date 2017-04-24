using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Nitra.ProjectSystem;
using Ammy.Language;
using File = System.IO.File;

namespace Ammy.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (!Debugger.IsAttached)
            //  Debugger.Launch();

            var sourceFiles = new List<string>();
            var referenceAssemblies = new List<string>();
            var outputPath = "";

            var referenceExpr = @"/reference:(.+)";
            var outputPathExpr = @"/outputPath:(.+)";
            var errorPrefix = "Ammy compiler error: ";

            foreach (var arg in args) {
                //Console.WriteLine(arg);

                var referenceMatch = Regex.Match(arg, referenceExpr);

                if (referenceMatch.Success && referenceMatch.Groups.Count == 2) {
                    referenceAssemblies.Add(referenceMatch.Groups[1].Value);
                    continue;
                }

                var outputPathExprMatch = Regex.Match(arg, outputPathExpr);
                if (outputPathExprMatch.Success && outputPathExprMatch.Groups.Count == 2) {
                    outputPath = outputPathExprMatch.Groups[1].Value;
                    continue;
                }

                if (File.Exists(arg))
                    sourceFiles.Add(arg);
                else {
                    Console.WriteLine($"{errorPrefix}Source file '" + arg + "' could not be found.");
                }
            }
            
            
        }
    }
}
