using System;
using System.Diagnostics;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio.Classifications
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(Strings.AmmyContentType)]
    public class ClassifierProvider : IClassifierProvider
    {
        [Import]
        IClassificationTypeRegistryService ClassificationRegistry;

        [Import]
        IClassificationFormatMapService _classificationFormatMapService;

        [Import]
        SVsServiceProvider serviceProvider;

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            ITextDocument textDocument;
            buffer.Properties.TryGetProperty(typeof (ITextDocument), out textDocument);

            IClassifier ammyClassifier;
            if (buffer.Properties.TryGetProperty(TextBufferProperties.AmmyClassifier, out ammyClassifier))
                return ammyClassifier;
            
            ammyClassifier = RuntimeLoader.GetClassifier(textDocument.FilePath, serviceProvider, buffer, (ITextDocument)textDocument, ClassificationRegistry, _classificationFormatMapService);
            buffer.Properties.AddProperty(TextBufferProperties.AmmyClassifier, ammyClassifier);
            return ammyClassifier;
        }

    }
}