namespace Ammy.Platforms
{
    public class CompilationResult
    {
        public string ProjectName { get; }
        public bool IsSuccess { get; }
        public CompilerMessage[] CompilerMessages { get; }
        public OutputFile[] Files { get; }

        public CompilationResult(string projectName, bool isSuccess, CompilerMessage[] compilerMessages, OutputFile[] files)
        {
            ProjectName = projectName;
            IsSuccess = isSuccess;
            CompilerMessages = compilerMessages;
            Files = files;
        }
    }
}