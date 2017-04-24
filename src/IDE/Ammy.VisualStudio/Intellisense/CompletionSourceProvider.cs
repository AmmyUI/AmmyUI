using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio.Intellisense
{
    [Export(typeof (ICompletionSourceProvider))]
    [ContentType(Strings.AmmyContentType)]
    [Name("ammy completion source provider")]
    public class CompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        SVsServiceProvider _serviceProvider;

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            ICompletionSource completionSource;
            textBuffer.Properties.TryGetProperty(typeof (ICompletionSource), out completionSource);

            if (completionSource != null)
                return completionSource;
            
            ITextDocument textDocument;
            textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument);

            completionSource = RuntimeLoader.CreateCompletionSource(textDocument.FilePath, _serviceProvider, textBuffer);
            textBuffer.Properties.AddProperty(typeof(ICompletionSource), completionSource);

            return completionSource;
        }
    }
}