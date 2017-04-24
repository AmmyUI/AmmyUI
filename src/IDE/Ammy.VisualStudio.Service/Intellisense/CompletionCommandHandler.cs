using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using IServiceProvider = System.IServiceProvider;

namespace Ammy.VisualStudio.Service.Intellisense
{
    public class CompletionCommandHandler : IOleCommandTarget
    {    
        // ReSharper disable once NotAccessedField.Local
        // don't change constructor signature without real need
        private readonly IServiceProvider _serviceProvider;
        private readonly ICompletionBroker _completionBroker;
        [Import] SVsServiceProvider ServiceProvider { get; set; }
        
        private readonly IOleCommandTarget _nextCommandHandler;
        private readonly CompletionController _completionController;
        private IList<IOleCommandTarget> _otherHandlers = new List<IOleCommandTarget>();
        private readonly CustomFormatter _customFormatter;

        public CompletionCommandHandler(IVsTextView textViewAdapter, ITextView textView, IServiceProvider serviceProvider, ITextStructureNavigatorSelectorService navigatorService, ISignatureHelpBroker signatureHelpBroker, ICompletionBroker completionBroker)
        {
            _serviceProvider = serviceProvider;
            _completionBroker = completionBroker;
            _completionController = new CompletionController(textView, navigatorService, signatureHelpBroker);
            _customFormatter = new CustomFormatter(textView, navigatorService);

            _otherHandlers.Add(new MixinSignatureHelpCommandHandler(textViewAdapter, textView, navigatorService.GetTextStructureNavigator(textView.TextBuffer), signatureHelpBroker));

            //add the command to the command chain
            textViewAdapter.AddCommandFilter(this, out _nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid cmdGroup, uint cmdId, uint cmdExecOpt, IntPtr pin, IntPtr pout)
        {
            if (VsShellUtilities.IsInAutomationFunction(ServiceProvider))
                return _nextCommandHandler.Exec(ref cmdGroup, cmdId, cmdExecOpt, pin, pout);
            
            var typedChar = char.MinValue;
            var retVal = VSConstants.S_OK;
            
            if (cmdGroup == VSConstants.VSStd2K && cmdId == (uint) VSConstants.VSStd2KCmdID.TYPECHAR)
                typedChar = (char) (ushort) Marshal.GetObjectForNativeVariant(pin);
            
            var shouldReturn = _completionController.TryCommitSession(typedChar, cmdId);
            if (shouldReturn)
                return retVal;
            
            if (cmdId == (uint)VSConstants.VSStd2KCmdID.RETURN) {
                if (_customFormatter.TryFormatOnEnter())
                    return VSConstants.S_OK;
            }

            var isControlSpace = Keyboard.Modifiers == ModifierKeys.Control && typedChar == ' ';
        
            if (!isControlSpace)
                retVal = _nextCommandHandler.Exec(ref cmdGroup, cmdId, cmdExecOpt, pin, pout);

            if (_completionController.TryComplete(typedChar, cmdId, _completionBroker))
                return VSConstants.S_OK;


            return retVal;
        }
    }
}