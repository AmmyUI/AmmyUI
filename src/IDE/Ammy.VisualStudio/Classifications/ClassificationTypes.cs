using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio.Classifications
{
    static class ClassificationTypes
    {
        [Export]
        [Name("ammy")]
        internal static ClassificationTypeDefinition MarkdownClassificationDefinition = null;
        
        [Export]
        [Name("Ammy Type")]
        [BaseDefinition("ammy")]
        internal static ClassificationTypeDefinition AmmyTypeDefinition = null;

        [Export]
        [Name("Ammy Property")]
        [BaseDefinition("ammy")]
        internal static ClassificationTypeDefinition AmmyPropertyFormat = null;

        [Export]
        [Name("Ammy Value")]
        [BaseDefinition("ammy")]
        internal static ClassificationTypeDefinition AmmyValueFormat = null;

        [Export]
        [Name("Ammy Mixin")]
        [BaseDefinition("ammy")]
        internal static ClassificationTypeDefinition AmmyMixinFormat = null;

        [Export]
        [Name("Ammy Keyword")]
        [BaseDefinition("ammy")]
        internal static ClassificationTypeDefinition AmmyKeywordFormat = null;

        [Export]
        [Name("Ammy Variable")]
        [BaseDefinition("ammy")]
        internal static ClassificationTypeDefinition AmmyVariableFormat = null;
    }
}