using Microsoft.VisualStudio.Text;
using Ammy.Build;

namespace Ammy.VisualStudio.Service.Compilation
{
    public class CompilationResponse
    {
        public CompileResult CompileResult { get; private set; }
        public ITextSnapshot Snapshot { get; }

        public CompilationResponse(CompileResult compileResult, ITextSnapshot snapshot)
        {
            CompileResult = compileResult;
            Snapshot = snapshot;
        }
    }
}