using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(Strings.AmmyTopMarginName)]
    [MarginContainer(PredefinedMarginNames.Top)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [ContentType(Strings.AmmyContentType)]
    public class TopMarginProvider : IWpfTextViewMarginProvider
    {
        [Import]
        private SVsServiceProvider serviceProvider;

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            ITextDocument textDocument;
            wpfTextViewHost.TextView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument);

            return RuntimeLoader.GetObjectByName<IWpfTextViewMargin>(textDocument.FilePath, "TopMargin", serviceProvider, wpfTextViewHost, marginContainer, serviceProvider);
        }
    }
}