namespace Ammy.Platforms
{
    public interface IAmmyCompiler
    {
        CompilationResult Compile(CompilationRequest request);
    }
}