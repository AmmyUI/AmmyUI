using Ammy.Build;
using Ammy.Language;
using EnvDTE;
using Nitra;

namespace Ammy.VisualStudio.Service
{
    public class Utils
    {
        public static bool MoveToLocation(Location location)
        {
            var projectItem = DteHelpers.GetProjectItemFromFilename(location.Source.File.FullName);
            var codeKind = Constants.vsext_vk_Code;

            if (projectItem != null) {
                var window = projectItem.Open(codeKind);
                window.Visible = true;
                var document = (TextDocument)window.Document.Object();
                var lineColumn = location.StartLineColumn;

                document.Selection.MoveToLineAndOffset(lineColumn.Line, lineColumn.Column);
                return true;
            }

            return false;
        }
    }
}