using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;
using Ammy.VisualStudio.Service.Extensions;
using Ammy.VisualStudio.Service.Settings;

namespace Ammy.VisualStudio.Service.Intellisense
{
    public class CustomFormatter
    {
        private readonly ITextView _textView;
        private readonly ITextStructureNavigatorSelectorService _navigatorService;

        public CustomFormatter(ITextView textView, ITextStructureNavigatorSelectorService navigatorService)
        {
            _textView = textView;
            _navigatorService = navigatorService;
        }
        
        public bool TryFormatOnEnter()
        {
            var caret = _textView.Caret;
            var line = caret.ContainingTextViewLine;
            var caretPosition = caret.Position.BufferPosition.Position;
            var caretPositionInLine = caretPosition - line.Start.Position;
            var lineText = line.Extent.GetText();
            var textAfterCaret = lineText.Substring(caretPositionInLine);
            var buffer = _textView.TextBuffer;
            var indent = lineText.GetIndent();
            
            var indentIncr = "  ";
            if (indent.FirstOrDefault() == '\t')
                indentIncr = "\t";
            
            var nlPlusIndent = Environment.NewLine + indent;

            if (IsInsideCurlyBrackets(lineText, textAfterCaret)) {
                if (!AmmySettings.OpeningBraceOnSameLine) {
                    while (caretPositionInLine > 0 && lineText[caretPositionInLine] != '{') {
                        caretPositionInLine--;
                        caretPosition--;
                    }
                    buffer.Delete(new Span(caretPosition, lineText.Length - caretPositionInLine));
                    var header = nlPlusIndent + "{" + nlPlusIndent + indentIncr;
                    var footer = nlPlusIndent + "}";
                    buffer.Insert(caretPosition, header + footer);
                    caret.MoveTo(new SnapshotPoint(buffer.CurrentSnapshot, caretPosition + header.Length));
                } else {
                    buffer.Delete(new Span(caretPosition, lineText.Length - caretPositionInLine));
                    buffer.Insert(caretPosition, nlPlusIndent + "}");
                    buffer.Insert(caretPosition, nlPlusIndent + indentIncr);
                    caret.MoveTo(new SnapshotPoint(buffer.CurrentSnapshot, caretPosition + nlPlusIndent.Length + indentIncr.Length));
                }
                
                return true;
            }

            if (IsAfterOpeningCurlyBracesBeforeNewLine(lineText, textAfterCaret)) {
                buffer.Insert(caretPosition, nlPlusIndent);
                caret.MoveTo(new SnapshotPoint(buffer.CurrentSnapshot, caretPosition + nlPlusIndent.Length));

                return true;
            }

            return false;
        }

        private static bool IsEmptyOrClosingBrace(string lineText, string closingBraceText)
        {
            if (string.IsNullOrWhiteSpace(lineText) || lineText.All(c => c == '\t'))
                return true;
            
            return lineText.StartsWith(closingBraceText);
        }

        private bool IsAfterOpeningCurlyBracesBeforeNewLine(string lineText, string textAfterCaret)
        {
            return Regex.IsMatch(lineText, @"\{[ \t]*$") && string.IsNullOrWhiteSpace(textAfterCaret);
        }

        private static bool IsInsideCurlyBrackets(string lineText, string textAfterCaret)
        {
            return Regex.IsMatch(lineText, @"\{[ \t]*\}[ \t]*$") && Regex.IsMatch(textAfterCaret, @"[ \t]*}[ \t]*$");
        }
    }
}