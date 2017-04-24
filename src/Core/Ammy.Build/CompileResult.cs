using System;
using System.Collections.Generic;
using System.Linq;
using Nitra;
using Nitra.ProjectSystem;
using Ammy.Language;

namespace Ammy.Build
{
    public class CompileResult
    {
        public object CompilationData { get; set; }
        public IList<string> GeneratedFiles { get; private set; } = new List<string>();
        public IList<string> GeneratedXamlFiles { get; private set; } = new List<string>();
        public IList<string> GeneratedBamlFiles { get; private set; } = new List<string>();
        public IList<AmmyFile<Top>> Files { get; } = new List<AmmyFile<Top>>();

        public IReadOnlyList<CompilerMessage> CompilerMessages {
            get {
                return Files.SelectMany(f => f.GetErrors())
                            .Concat(_externalErrors)
                            .ToArray();
            }
        }

        public AmmyProject AmmyProject { get; set; }
        public XamlProjectMeta ProjectMeta { get; } = new XamlProjectMeta();
        public bool NeedBamlGeneration { get; private set; }
        public bool IsSuccess => !CompilerMessages.Any(IsError);
        private readonly List<CompilerMessage> _externalErrors = new List<CompilerMessage>();
        public string ProjectName { get; set; }

        private static bool IsError(CompilerMessage msg)
        {
            return msg.Type == CompilerMessageType.Error || msg.Type == CompilerMessageType.FatalError;
        }

        public CompileResult(bool needBamlGeneration)
        {
            NeedBamlGeneration = needBamlGeneration;
        }

        public void AddError(string message)
        {
            var compilerMessage = new CompilerMessage(CompilerMessageType.FatalError, Guid.Empty, Location.Default, message, 0, new List<CompilerMessage>());
            _externalErrors.Add(compilerMessage);
        }
        
        public AmmyFile<Top> GetFile(string filename, int version = -1)
        {
            foreach (var file in Files) {
                if (file.FullName == null)
                    continue;

                if (version == -1 && file.FullName.Equals(filename, StringComparison.InvariantCultureIgnoreCase))
                    return file;

                var meta = file.Meta;
                
                if (file.FullName.Equals(filename, StringComparison.InvariantCultureIgnoreCase) && meta.Version == version)
                    return file;
            }

            return null;
        }
    }
}