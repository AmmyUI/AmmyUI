using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Ammy.Build;
using Ammy.Infrastructure;

namespace Ammy.VisualStudio.Service.Compilation
{
    public class CompilerServiceRuntimeUpdate
    {
        private readonly CompilerService _compilerService;

        public CompilerServiceRuntimeUpdate(CompilerService compilerService)
        {
            _compilerService = compilerService;
        }

        public void SendRuntimeUpdate(CompileResult result, string objAbsolutePath, string assemblyName)
        {
            var ammyMetaFilename = Path.Combine(objAbsolutePath, "ammy.meta");

            if (_compilerService.PreviousUpdateMeta == null && File.Exists(ammyMetaFilename)) {
                using (var stream = File.OpenRead(ammyMetaFilename)) {
                    var serializer = new XmlSerializer(typeof(XamlProjectMeta));
                    _compilerService.PreviousUpdateMeta = serializer.Deserialize(stream) as XamlProjectMeta;
                }
            }

            var previousMeta = _compilerService.PreviousUpdateMeta;
            if (previousMeta == null) {
                Debug.WriteLine("Error: Couldn't get previous update meta");
                return;
            }

            var currentMeta = result.ProjectMeta;
            var changedFiles = GetChangedNodes(currentMeta, previousMeta);

            _compilerService.PreviousUpdateMeta = result.ProjectMeta;

            Debug.WriteLine("Sending runtime update: " + string.Join(", ", changedFiles));
            
            RuntimeUpdateSender.Send(result.AmmyProject.PlatformName, changedFiles, previousMeta, $"/{assemblyName};component/");
        }
        
        private static List<XamlFileMeta> GetChangedNodes(XamlProjectMeta currentMeta, XamlProjectMeta previousUpdateMeta)
        {
            var changedFiles = new List<XamlFileMeta>();

            foreach (var file in currentMeta.Files) {
                var xamlFilename = file.Filename;
                var previousFileMeta = previousUpdateMeta.Files.FirstOrDefault(f => f.Filename.SamePathAs(xamlFilename));

                if (previousFileMeta == null)
                    continue;

                if (file.Hash != previousFileMeta.Hash)
                    changedFiles.Add(file);
            }

            return changedFiles;
        }
    }
}