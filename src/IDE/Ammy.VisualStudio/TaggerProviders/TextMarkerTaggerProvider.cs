using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio.Errors
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(Strings.AmmyContentType)]
    [TagType(typeof(TextMarkerTag))]
    class TextMarkerTaggerProvider : IViewTaggerProvider
    {
        [Import]
        SVsServiceProvider serviceProvider;
        
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            ITextDocument textDocument;
            buffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument);

            Func<ITagger<T>> sc = delegate { return RuntimeLoader.GetObject<ITagger<T>>(textDocument.FilePath, t => t.Name == "TextMarkerTagger", serviceProvider, serviceProvider, buffer, textView); };
            return buffer.Properties.GetOrCreateSingletonProperty(sc);
        }
    }
}