using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio.Intellisense
{
    [Export(typeof(ISmartTagSourceProvider))]
    [Name("Smart tag source")]
    [ContentType(Strings.AmmyContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class SmartTagSourceProvider : ISmartTagSourceProvider
    {
        [Import]
        SVsServiceProvider serviceProvider;

        public ISmartTagSource TryCreateSmartTagSource(ITextBuffer textBuffer)
        {
            ITextDocument textDocument;
            textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument);

            Func<ISmartTagSource> sc = delegate { return RuntimeLoader.GetObject<ISmartTagSource>(textDocument.FilePath, t => t.Name == "SmartTagSource", serviceProvider, textBuffer); };
            return textBuffer.Properties.GetOrCreateSingletonProperty(sc);
        }
    }
}