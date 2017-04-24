using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio.TaggerProviders
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(Strings.AmmyContentType)]
    [TagType(typeof(IntraTextAdornmentTag))]
    class IntraTextAdornmentTaggerProvider : ITaggerProvider
    {
        [Import]
        SVsServiceProvider _serviceProvider;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITextDocument textDocument;
            buffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument);

            Func<ITagger<T>> sc = delegate { return RuntimeLoader.GetObject<ITagger<T>>(textDocument.FilePath, t => t.Name == "IntraTextAdornmentTagger", _serviceProvider, buffer); };
            return buffer.Properties.GetOrCreateSingletonProperty(sc);
        }
    }
}