using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Input;
using Ammy.Build;
using Ammy.Language;
using Ammy.VisualStudio.Service.Compilation;
using Ammy.VisualStudio.Service.Intellisense;
using EnvDTE;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Nitra;
using Nitra.Declarations;

namespace Ammy.VisualStudio.Service.Mouse
{
    public class MouseProcessor : MouseProcessorBase
    {
        private readonly IWpfTextView _wpfTextView;
        private readonly CompilerService _compilerService = CompilerService.Instance;
        private readonly ITextDocument _document;

        public MouseProcessor(IWpfTextView wpfTextView)
        {
            _wpfTextView = wpfTextView;
            wpfTextView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out _document);
        }
    }
}