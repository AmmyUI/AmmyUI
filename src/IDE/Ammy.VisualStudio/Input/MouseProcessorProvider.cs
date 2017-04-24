using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio.Input
{
    [Export(typeof (IMouseProcessorProvider))]
    [Name("Mouse processor")]
    [ContentType(Strings.AmmyContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class MouseProcessorProvider : IMouseProcessorProvider
    {
        [Import]
        SVsServiceProvider serviceProvider;

        public IMouseProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
        {
            ITextDocument textDocument;
            wpfTextView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument);

            Func<IMouseProcessor> sc = delegate { return RuntimeLoader.GetObject<IMouseProcessor>(textDocument.FilePath, t => t.Name == "MouseProcessor", serviceProvider, wpfTextView); };
            return wpfTextView.Properties.GetOrCreateSingletonProperty(sc);
        }
    }
}