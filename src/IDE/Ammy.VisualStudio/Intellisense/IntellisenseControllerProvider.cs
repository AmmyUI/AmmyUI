using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio.Intellisense
{
    [Export(typeof(IIntellisenseControllerProvider))]
    [Name("Intellisense Controller Provider")]
    [Order(Before = "default")]
    [ContentType(Strings.AmmyContentType)]
    internal class IntellisenseControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        SVsServiceProvider _serviceProvider;

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            ITextDocument textDocument;
            textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument);

            Func<IIntellisenseController> sc = delegate { return RuntimeLoader.GetObject<IIntellisenseController>(textDocument.FilePath, t => t.Name == "IntellisenseController", _serviceProvider, textView, subjectBuffers); };
            return textView.Properties.GetOrCreateSingletonProperty(sc);
        }
    }
}