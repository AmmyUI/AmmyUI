namespace Ammy.Platforms
{
    public class OutputFile
    {
        public string FullPath { get; }
        public string Xaml { get; }

        public OutputFile(string fullPath, string xaml)
        {
            FullPath = fullPath;
            Xaml = xaml;
        }
    }
}