using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio.Classifications
{
    static class ClassificationFormats
    {
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "Ammy Type")]
        [Name("Ammy Type")]
        [UserVisible(true)]
        sealed class AmmyTypeFormat : ClassificationFormatDefinition
        {
            public AmmyTypeFormat()
            {
                ForegroundBrush = new SolidColorBrush(Color.FromRgb(74, 140, 43));
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "Ammy Property")]
        [Name("Ammy Property")]
        [UserVisible(true)]
        sealed class AmmyPropertyFormat : ClassificationFormatDefinition
        {
            public AmmyPropertyFormat()
            {
                ForegroundBrush = new SolidColorBrush(Color.FromRgb(56, 140, 158));
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "Ammy Value")]
        [Name("Ammy Value")]
        [UserVisible(true)]
        sealed class AmmyValueFormat : ClassificationFormatDefinition
        {
            public AmmyValueFormat()
            {
                ForegroundBrush = new SolidColorBrush(Color.FromRgb(82, 101, 100));
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "Ammy Mixin")]
        [Name("Ammy Mixin")]
        [UserVisible(true)]
        sealed class AmmyMixinFormat : ClassificationFormatDefinition
        {
            public AmmyMixinFormat()
            {
                ForegroundBrush = new SolidColorBrush(Color.FromRgb(167, 46, 49));
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "Ammy Keyword")]
        [Name("Ammy Keyword")]
        [UserVisible(true)]
        sealed class AmmyKeywordFormat : ClassificationFormatDefinition
        {
            public AmmyKeywordFormat()
            {
                ForegroundBrush = new SolidColorBrush(Color.FromRgb(63, 117, 171));
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "Ammy Variable")]
        [Name("Ammy Variable")]
        [UserVisible(true)]
        sealed class AmmyVariableFormat : ClassificationFormatDefinition
        {
            public AmmyVariableFormat()
            {
                ForegroundBrush = new SolidColorBrush(Color.FromRgb(181, 137, 36));
            }
        }
    }
}