using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio.Intellisense
{
    [Export(typeof(IQuickInfoSourceProvider))]
    [Name("Mouse processor")]
    [ContentType(Strings.AmmyContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class QuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import]
        SVsServiceProvider serviceProvider;

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            ITextDocument textDocument;
            textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument);

            Func<IQuickInfoSource> sc = delegate { return RuntimeLoader.GetObject<IQuickInfoSource>(textDocument.FilePath, t => t.Name == "QuickInfoSource", serviceProvider, textBuffer); };
            return textBuffer.Properties.GetOrCreateSingletonProperty(sc);
        }
    }
}