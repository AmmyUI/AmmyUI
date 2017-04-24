using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Ammy.VisualStudio
{
    internal static class FileAndContentTypeDefinitions
    {
        [Export]
        [Name(Strings.AmmyContentType)]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition ammyContentTypeDefinition;

        [Export]
        [FileExtension(Strings.AmmyExtension)]
        [ContentType(Strings.AmmyContentType)]
        internal static FileExtensionToContentTypeDefinition ammyFileExtensionDefinition;
    }



    internal static class Strings
    {
        public const string AmmyExtension = ".ammy";
        public const string AmmyContentType = "ammy";
        public const string AmmyTopMarginName = "AmmyTopMargin";
        public const string AmmyBottomMarginName = "AmmyBottomMargin";
    }
}
