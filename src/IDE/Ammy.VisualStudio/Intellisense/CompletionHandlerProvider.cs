using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio.Intellisense
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("ammy completion handler source")]
    [ContentType(Strings.AmmyContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    public class CompletionHandlerProvider : IVsTextViewCreationListener
    {
        [Import] IVsEditorAdaptersFactoryService _adapterService;
        [Import] SVsServiceProvider _serviceProvider;
        [Import] ITextStructureNavigatorSelectorService _navigatorService;
        [Import] ISignatureHelpBroker _signatureHelpBroker;
        [Import] ICompletionBroker _completionBroker;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = _adapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            ITextDocument textDocument;
            textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument);

            object commandHandler;
            if (!textView.Properties.TryGetProperty("CompletionCommandHandler", out commandHandler)) {
                var handler = RuntimeLoader.CreateCommandHandler(textDocument.FilePath, _serviceProvider, textViewAdapter, textView, _navigatorService, _signatureHelpBroker, _completionBroker);
                textView.Properties.AddProperty("CompletionCommandHandler", handler);
            }
        }
    }
}