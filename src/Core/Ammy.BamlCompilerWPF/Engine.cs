using System;
using System.Collections;
using Microsoft.Build.Framework;

namespace Ammy.BamlCompilerWPF
{
    class Engine : IBuildEngine
    {
        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            Console.WriteLine($"{e.File}({e.LineNumber},{e.ColumnNumber},{e.EndLineNumber},{e.EndColumnNumber})|Error {e.Code}|{e.Message}|_e");
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            Console.WriteLine("Warning: " + e.Message);
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            Console.WriteLine("Message: " + e.Message);
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            Console.WriteLine(e);
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
        {
            throw new NotImplementedException();
        }

        public bool ContinueOnError { get; }
        public int LineNumberOfTaskNode { get; }
        public int ColumnNumberOfTaskNode { get; }
        public string ProjectFileOfTaskNode { get; }
    }
}