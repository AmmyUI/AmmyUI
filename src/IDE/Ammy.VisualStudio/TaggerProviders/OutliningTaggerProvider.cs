using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio.Errors
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(Strings.AmmyContentType)]
    [TagType(typeof(OutliningRegionTag))]
    class OutliningTaggerProvider : ITaggerProvider
    {
        [Import]
        SVsServiceProvider serviceProvider;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITextDocument textDocument;
            buffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument);

            Func<ITagger<T>> sc = delegate { return RuntimeLoader.GetObject<ITagger<T>>(textDocument.FilePath, t => t.Name == "OutliningTagger", serviceProvider, buffer); };
            return buffer.Properties.GetOrCreateSingletonProperty(sc);
        }
    }
}