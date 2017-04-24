namespace Ammy.Platforms
{
    public class CompilerMessage
    {
        public CompilerMessageType Type { get; }
        public string Message { get; }
        public CompilerMessageLocation Location { get; set; }

        public CompilerMessage(CompilerMessageType type, string message, CompilerMessageLocation location)
        {
            Type = type;
            Message = message;
            Location = location;
        }
    }

    public class CompilerMessageLocation
    {
        public int Row { get; }
        public int Column { get; }
        public string Filename { get; }

        public CompilerMessageLocation(int row, int column, string filename)
        {
            Row = row;
            Column = column;
            Filename = filename;
        }
    }
}