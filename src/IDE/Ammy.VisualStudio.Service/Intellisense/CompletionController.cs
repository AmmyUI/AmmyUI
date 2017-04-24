using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Input;
using Ammy.Language;
using Ammy.VisualStudio.Service.Compilation;
using Ammy.VisualStudio.Service.Extensions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Nitra;

namespace Ammy.VisualStudio.Service.Intellisense
{
    public class CompletionController
    {
        private readonly ITextView _textView;
        private readonly ITextStructureNavigatorSelectorService _navigatorService;
        private readonly ISignatureHelpBroker _signatureHelpBroker;
        private readonly CompilerService _compilerService = CompilerService.Instance;
        private readonly ITextDocument _document;

        private ICompletionSession _session;
        
        public bool IsSessionAlive => _session != null && !_session.IsDismissed;

        public CompletionController(ITextView textView, ITextStructureNavigatorSelectorService navigatorService, ISignatureHelpBroker signatureHelpBroker)
        {
            _textView = textView;
            _navigatorService = navigatorService;
            _signatureHelpBroker = signatureHelpBroker;
            
            textView.TextBuffer.Properties.TryGetProperty(typeof (ITextDocument), out _document);
        }

        public bool TryCommitSession(char typedChar, uint commandId)
        {
            if (ShouldCommitSession(commandId, typedChar))
                if (CommitSession(typedChar))
                    return true;

            return false;
        }

        public bool TryComplete(char typedChar, uint commandId, ICompletionBroker completionBroker, bool isRecursiveCall = false)
        {
            if (IsSessionStartSequence(typedChar)) {
                bool waitForCompilation;
                if (ShouldStartSession(typedChar, _document.FilePath, _textView.Caret.Position.BufferPosition, isRecursiveCall, out waitForCompilation)) {
                    TriggerCompletion(completionBroker);
                } else if (waitForCompilation && !isRecursiveCall) {
                    _compilerService.Compilations
                                    .Take(1)
                                    .ObserveOn(SynchronizationContext.Current)
                                    .Subscribe(_ => TryComplete(typedChar, commandId, completionBroker, true));
                } else {
                    if (IsSessionAlive)
                        _session.Filter();
                }
                return true;
            }
            
            var session = _session;

            if (IsDeletion(commandId) && session != null) {
                var applicableTo = session.SelectedCompletionSet?.ApplicableTo;

                if (applicableTo != null) {
                    var text = applicableTo.GetText(applicableTo.TextBuffer.CurrentSnapshot);

                    if (string.IsNullOrWhiteSpace(text))
                        session.Dismiss();
                    else if (!session.IsDismissed)
                        session.Filter();
                }
                return true;
            }

            if (IsSessionAlive) {
                var applicableTo = _session?.SelectedCompletionSet?.ApplicableTo;
                if (applicableTo != null && applicableTo.GetSpan(_textView.TextSnapshot).IsEmpty) {
                    _session.Dismiss();
                }
            }

            return false;
        }

        private bool ShouldStartSession(char typedChar, string filename, SnapshotPoint caretPosition, bool isRecursiveCall, out bool waitForCompilation)
        {
            waitForCompilation = false;

            if (_session != null && !_session.IsDismissed)
                return false;

            var stack = _compilerService.GetCurrentAstStack(filename, caretPosition)
                                        .Where(ast => !(ast is IEnumerable)) // Skip IAstList
                                        .ToArray();

            if (stack.Length == 0)
                return false;

            var stackTop = stack[0];

            if (stackTop is Property || (typedChar == '.' && !isRecursiveCall)) {
                waitForCompilation = true;
                return false;
            }

            if (stackTop is GlobalDeclaration)
                return false;

            return true;
        }

        private static bool IsDeletion(uint commandId)
        {
            return commandId == (uint)VSConstants.VSStd2KCmdID.BACKSPACE
                || commandId == (uint)VSConstants.VSStd2KCmdID.DELETE;
        }

        private void TriggerCompletion(ICompletionBroker completionBroker)
        {
            if (_session != null)
                return;
            
            var caret = _textView.Caret;
            var caretPosition = caret.Position;
            var caretPoint = caretPosition.Point.GetPoint(textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);

            if (!caretPoint.HasValue)
                return;

            var snapshot = caretPoint.Value.Snapshot;
            var trackingPoint = snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive);

            _session = completionBroker.CreateCompletionSession(_textView, trackingPoint, false);
            _session.Dismissed += OnSessionDismissed;
            _session.Start();

            if (IsSessionAlive)
                _session.Filter();
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            _session.Dismissed -= OnSessionDismissed;
            _session = null;
        }

        private static bool IsSessionStartSequence(char typedChar)
        {
            var isControlSpace = Keyboard.Modifiers == ModifierKeys.Control && typedChar == ' ';
            return IsCompletionStartSymbol(typedChar) || typedChar == '.';// || isControlSpace;
        }
        
        private static bool IsCompletionStartSymbol(char typedChar)
        {
            return (!typedChar.Equals(char.MinValue) &&
                   (char.IsLetter(typedChar) || typedChar == '@' || typedChar == '#' || typedChar == '$'));
        }

        private bool CommitSession(int typedChar)
        {
            var completionSet = (MyCompletionSet)_session.SelectedCompletionSet;
            var selectionStatus = completionSet.SelectionStatus;
            var applicableTo = completionSet.ApplicableTo;
            var currentSnapshot = applicableTo.TextBuffer.CurrentSnapshot;

            if (applicableTo.GetText(currentSnapshot).StartsWith(".")) {
                var span = applicableTo.GetSpan(currentSnapshot);

                if (span.Start.Position + 1 < currentSnapshot.Length - 1) {
                    var newSpan = new SnapshotSpan(currentSnapshot, span.Start.Position + 1, span.Length - 1);
                    completionSet.UpdateApplicableTo(currentSnapshot.CreateTrackingSpan(newSpan, SpanTrackingMode.EdgeInclusive));
                }
            }

            if (selectionStatus.IsSelected) {
                _session.Commit();

                var completion = selectionStatus.Completion as MyCompletion;
                var position = _textView.Caret.Position.BufferPosition.Position;
                var buffer = _textView.TextBuffer;

                if (typedChar == '.') {
                    //buffer.Insert(position, ".");
                } else if (completion != null) {
                    var stack = _compilerService.GetCurrentAstStack(_document.FilePath, position);

                    if (!stack.IsInside<LambdaExpr>())
                        SmartComplete(completion, applicableTo.GetStartPoint(currentSnapshot).Position, position, buffer);
                }

                if (typedChar != '.')
                    return true;
            }

            _session?.Dismiss();
            return false;
        }

        private void SmartComplete(MyCompletion completion, int positionBefore, int positionAfter, ITextBuffer buffer)
        {
            var completionType = completion.CompletionType;
            var newLineText = GetNewLineText(new SnapshotPoint(_textView.TextSnapshot, positionAfter));
            var currentSnapshot = buffer.CurrentSnapshot;

            var textAfterCaret = positionAfter < currentSnapshot.Length 
                                 ? currentSnapshot.GetText(positionAfter, (currentSnapshot.Length - (positionAfter + 1))) 
                                 : "";

            var textBeforeCaret = currentSnapshot.GetText(0, positionBefore);

            // Need to map to PrettyPrint output without comments and other insignificant trash
            var nextSymbol = textAfterCaret.FirstOrDefault(c => c != ' ' && c != '\r' && c != '\n');
            var prevSymbol = textBeforeCaret.Reverse().FirstOrDefault(c => c != ' ');

            var line = _textView.Caret.ContainingTextViewLine;
            var lineText = line.Extent.GetText();
            var indent = string.Join("", lineText.TakeWhile(c => char.IsWhiteSpace(c) || c == '\t'));

            if (completionType == CompletionType.Property && prevSymbol != ':' && nextSymbol != ':') {
                SmartCompleteProperty(positionAfter, buffer);
            } else if (completionType == CompletionType.Node && nextSymbol != '{' && !completion.IsPropertyValue) {
                SmartCompleteNode(positionAfter, buffer, newLineText, indent);
            } else if (completionType == CompletionType.ContentFunctionRef && nextSymbol != '(') {
                //SmartCompleteContentFunctionRef(positionAfter, buffer, newLineText, indent);
            } else if (completionType == CompletionType.TypeFunctionRef && nextSymbol != '(') {
                SmartCompleteTypeFunctionRef(positionAfter, buffer, newLineText, indent);
            } else if (completionType == CompletionType.ContentFunctionRefWithParams && nextSymbol != '(') {
                SmartCompleteContentFunctionRefWithParams(positionAfter, buffer, newLineText, indent);
            } else if (completionType == CompletionType.TypeFunctionRefWithParams && nextSymbol != '(') {
                SmartCompleteTypeFunctionRefWithParams(positionAfter, buffer, newLineText, indent);
            }
        }

        private static void SmartCompleteProperty(int positionAfter, ITextBuffer buffer)
        {
            buffer.Insert(positionAfter, ": ");
        }

        private static void SmartCompleteContentFunctionRef(int positionAfter, ITextBuffer buffer, string newLineText, string indent)
        {
            buffer.Insert(positionAfter, "()");
        }

        private void SmartCompleteTypeFunctionRefWithParams(int positionAfter, ITextBuffer buffer, string newLineText, string indent)
        {
            var beforeCaret = "() {";
            var afterCaret = "}";

            buffer.Insert(positionAfter, beforeCaret + afterCaret);

            _textView.Caret.MoveTo(new SnapshotPoint(buffer.CurrentSnapshot, positionAfter + 1));
            _compilerService.AfterNextCompilation(_ => _signatureHelpBroker.TriggerSignatureHelp(_textView), SynchronizationContext.Current);
        }

        private void SmartCompleteContentFunctionRefWithParams(int positionAfter, ITextBuffer buffer, string newLineText, string indent)
        {
            buffer.Insert(positionAfter, "()");

            _textView.Caret.MoveTo(new SnapshotPoint(buffer.CurrentSnapshot, positionAfter + 1));
            _compilerService.AfterNextCompilation(_ => _signatureHelpBroker.TriggerSignatureHelp(_textView), SynchronizationContext.Current);
        }

        private void SmartCompleteTypeFunctionRef(int positionAfter, ITextBuffer buffer, string newLineText, string indent)
        {
            var beforeCaret = "() {";
            var afterCaret = " }";
            buffer.Insert(positionAfter, beforeCaret + afterCaret);
            _textView.Caret.MoveTo(new SnapshotPoint(buffer.CurrentSnapshot, positionAfter + beforeCaret.Length));
        }

        private void SmartCompleteNode(int positionAfter, ITextBuffer buffer, string newLineText, string offsetText)
        {
            var beforeCaret = " { ";
            var afterCaret =  " }";

            buffer.Insert(positionAfter, beforeCaret + afterCaret);

            _textView.Caret.MoveTo(new SnapshotPoint(buffer.CurrentSnapshot, positionAfter + beforeCaret.Length));
        }

        private bool ShouldCommitSession(uint commandId, char typedChar)
        {
            var isCommitSequence = commandId == (uint) VSConstants.VSStd2KCmdID.RETURN
                                || commandId == (uint) VSConstants.VSStd2KCmdID.TAB
                                || (char.IsWhiteSpace(typedChar) || char.IsPunctuation(typedChar));

            return isCommitSequence && IsSessionAlive;
        }

        private static string GetNewLineText(SnapshotPoint point)
        {
            var line = point.GetContainingLine();

            if (line.LineBreakLength > 0)
                return line.GetLineBreakText();

            if (line.LineNumber - 1 >= 0) {
                var lineAbove = line.Snapshot.GetLineFromLineNumber(line.LineNumber - 1);
                return lineAbove.GetLineBreakText();
            }

            return Environment.NewLine;
        }
    }
}