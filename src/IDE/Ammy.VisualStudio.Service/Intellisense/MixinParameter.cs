using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Ammy.VisualStudio.Service.Intellisense
{
    public class MixinParameter : IParameter
    {
        public MixinParameter(string documentation, Span locus, string name, ISignature signature)
        {
            Documentation = documentation;
            Locus = locus;
            Name = name;
            Signature = signature;
        }

        public ISignature Signature { get; }
        public string Name { get; }
        public string Documentation { get; }
        public Span Locus { get; }
        public Span PrettyPrintedLocus { get; }
    }
}