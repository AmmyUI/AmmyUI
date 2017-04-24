using Ammy.Build;
using Ammy.Language;

namespace Ammy.VisualStudio.Service.Compilation
{
    public interface ICompilationListener
    {
        string FilePath { get; }
        void Update(AmmyFile<Top> file);
    }
}