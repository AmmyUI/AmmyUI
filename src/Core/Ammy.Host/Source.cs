namespace Ammy.Platforms
{
    public abstract class Source
    {
        public string Path { get; protected set; }
    }

    public class FileSource : Source
    {
        public FileSource(string path)
        {
            Path = path;
        }
    }

    public class TextSource : Source
    {
        public string SourceText { get; }

        public TextSource(string path, string sourceText)
        {
            Path = path;
            SourceText = sourceText;
        }
    }
}