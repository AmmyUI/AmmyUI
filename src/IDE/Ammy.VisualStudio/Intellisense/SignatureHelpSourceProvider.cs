using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio.Intellisense
{
    [Export(typeof(ISignatureHelpSourceProvider))]
    [Name("Mouse processor")]
    [ContentType(Strings.AmmyContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class SignatureHelpSourceProvider : ISignatureHelpSourceProvider
    {
        [Import]
        SVsServiceProvider _serviceProvider;
        
        public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer)
        {
            ITextDocument textDocument;
            textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument);

            Func<ISignatureHelpSource> sc = delegate { return RuntimeLoader.GetObject<ISignatureHelpSource>(textDocument.FilePath, t => t.Name == "SignatureHelpSource", _serviceProvider, textBuffer); };
            return textBuffer.Properties.GetOrCreateSingletonProperty(sc);
        }
    }
}