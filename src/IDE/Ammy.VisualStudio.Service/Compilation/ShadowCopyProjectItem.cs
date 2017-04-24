using Microsoft.VisualStudio.Text;

namespace Ammy.VisualStudio.Service.Compilation
{
    public class ShadowCopyProjectItem
    {
        public string OriginalFilename { get; private set; }
        public string TemporaryFilename { get; private set; }
        public ITextBuffer Buffer { get; set; }

        public ShadowCopyProjectItem(string filename, string originalFilename)
        {
            TemporaryFilename = filename;
            OriginalFilename = originalFilename;
        }

        public int GetVersion()
        {
            return Buffer?.CurrentSnapshot.Version.VersionNumber ?? 0;
        }
    }
}