//------------------------------------------------------------------------------
// <copyright file="AdornmentTextViewCreationListener.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio.Adornments
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(Strings.AmmyContentType)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class AdornmentTextViewCreationListener : IWpfTextViewCreationListener
    {
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("AdornmentLayer0")]
        [Order(After = PredefinedAdornmentLayers.Selection, Before = PredefinedAdornmentLayers.Text)]
        private AdornmentLayerDefinition editorAdornmentLayer;

        [Import]
        SVsServiceProvider serviceProvider;

        public void TextViewCreated(IWpfTextView textView)
        {
            ITextDocument textDocument;
            textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument);

            Func<object> sc = delegate { return RuntimeLoader.GetObject<object>(textDocument.FilePath, t => t.Name == "WpfTextViewHandler", serviceProvider, textView); };
            textView.Properties.GetOrCreateSingletonProperty(sc);
        }
    }
}
