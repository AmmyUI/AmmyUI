using System;
using Ammy.Language;

namespace Ammy.Build
{
    public class AmmyFileMeta
    {
        public string Filename { get; }
        public object Snapshot { get; private set; }
        public string SourceText { get; }
        public int Version { get; private set; }
        public AmmyFile<Top> File { get; set; }
        public object ProjectItem { get; }

        public AmmyFileMeta(string filename, object projectItem = null)
        {
            Filename = filename;
            Version = -1;
            ProjectItem = projectItem;
        }

        public AmmyFileMeta(string filename, object snapshot, string sourceText, int version, object projectItem = null)
        {
            Filename = filename;
            Snapshot = snapshot;
            SourceText = sourceText;
            Version = version;
            ProjectItem = projectItem;
        }

        public void UpdateSnapshot(object snapshot, int version)
        {
            Snapshot = snapshot;
            Version = version;
        }
    }
}