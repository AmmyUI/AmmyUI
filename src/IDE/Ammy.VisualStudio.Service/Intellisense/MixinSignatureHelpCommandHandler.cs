using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Ammy.VisualStudio.Service.Intellisense
{
    internal sealed class MixinSignatureHelpCommandHandler : IOleCommandTarget
    {
        private readonly ISignatureHelpBroker _broker;
        private readonly ITextStructureNavigator _navigator;
        private readonly IOleCommandTarget _nextCommandHandler;
        private readonly ITextView _textView;
        private ISignatureHelpSession _session;

        internal MixinSignatureHelpCommandHandler(IVsTextView textViewAdapter, ITextView textView, ITextStructureNavigator nav, ISignatureHelpBroker broker)
        {
            _textView = textView;
            _broker = broker;
            _navigator = nav;

            //add this to the filter chain
            textViewAdapter.AddCommandFilter(this, out _nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint) VSConstants.VSStd2KCmdID.TYPECHAR) {
                var typedChar = (char) (ushort) Marshal.GetObjectForNativeVariant(pvaIn);
                if (typedChar.Equals('(')) {
                    //var point = _textView.Caret.Position.BufferPosition - 1;
                    //var extent = _navigator.GetExtentOfWord(point);
                    //var word = extent.Span.GetText();
                    _session = _broker.TriggerSignatureHelp(_textView);
                } else if (typedChar.Equals(')') && _session != null) {
                    _session.Dismiss();
                    _session = null;
                }
            }
            return _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }
    }
}