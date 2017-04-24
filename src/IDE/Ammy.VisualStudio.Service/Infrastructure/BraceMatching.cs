using System;
using System.Collections.Generic;
using System.Linq;
using Ammy.VisualStudio.Service.Taggers;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Ammy.VisualStudio.Service.Infrastructure
{
    public class BraceMatching : INeedLogging
    {
        private readonly Dictionary<char, char> _braceList;
        private readonly ITextBuffer _buffer;
        private readonly TextMarkerTagger _tagger;
        private readonly ITextView _textView;

        private SnapshotPoint? _currentChar;

        public BraceMatching(TextMarkerTagger tagger, ITextBuffer buffer, ITextView textView)
        {
            _tagger = tagger;
            _buffer = buffer;
            _textView = textView;

            _braceList = new Dictionary<char, char> { { '{', '}' }, { '[', ']' }, { '(', ')' } };
            _currentChar = null;

            _textView.Caret.PositionChanged += CaretPositionChanged;
            _textView.LayoutChanged += ViewLayoutChanged;
        }

        private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.NewSnapshot != e.OldSnapshot)
                UpdateAtCaretPosition(_textView.Caret.Position);
        }

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            UpdateAtCaretPosition(e.NewPosition);
        }

        private void UpdateAtCaretPosition(CaretPosition position)
        {
            _currentChar = position.Point.GetPoint(_buffer, position.Affinity);

            if (!_currentChar.HasValue)
                return;

            var snapshot = _buffer.CurrentSnapshot;
            _tagger.RaiseTagsChanged(new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
        }

        public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var result = new List<ITagSpan<TextMarkerTag>>();

            try {
                if (spans.Count == 0) //there is no content in the buffer
                    return result;

                //don't do anything if the current SnapshotPoint is not initialized or at the end of the buffer
                if (!_currentChar.HasValue || _currentChar.Value.Position >= _currentChar.Value.Snapshot.Length)
                    return result;

                //hold on to a snapshot of the current character
                var currentChar = _currentChar.Value;

                //if the requested snapshot isn't the same as the one the brace is on, translate our spans to the expected snapshot
                if (spans[0].Snapshot != currentChar.Snapshot)
                    currentChar = currentChar.TranslateTo(spans[0].Snapshot, PointTrackingMode.Positive);

                //get the current char and the previous char
                var currentText = currentChar.GetChar();
                var lastChar = currentChar == 0 ? currentChar : currentChar - 1; //if currentChar is 0 (beginning of buffer), don't move it back
                var lastText = lastChar.GetChar();
                var pairSpan = new SnapshotSpan();

                if (_braceList.ContainsKey(currentText)) //the key is the open brace
                {
                    char closeChar;
                    _braceList.TryGetValue(currentText, out closeChar);
                    if (FindMatchingCloseChar(currentChar, currentText, closeChar, _textView.TextViewLines.Count, out pairSpan)) {
                        result.Add(new TagSpan<TextMarkerTag>(new SnapshotSpan(currentChar, 1), new TextMarkerTag("blue")));
                        result.Add(new TagSpan<TextMarkerTag>(pairSpan, new TextMarkerTag("blue")));
                    }
                } else if (_braceList.ContainsValue(lastText)) //the value is the close brace, which is the *previous* character 
                {
                    var open = from n in _braceList
                        where n.Value.Equals(lastText)
                        select n.Key;
                    if (FindMatchingOpenChar(lastChar, open.ElementAt(0), lastText, _textView.TextViewLines.Count, out pairSpan)) {
                        result.Add(new TagSpan<TextMarkerTag>(new SnapshotSpan(lastChar, 1), new TextMarkerTag("blue")));
                        result.Add(new TagSpan<TextMarkerTag>(pairSpan, new TextMarkerTag("blue")));
                    }
                }
                return result;
            } catch (Exception e) {
                this.LogDebugInfo("BraceMatching GetTags failed: " + e);
                return result;
            }
        }

        private static bool FindMatchingCloseChar(SnapshotPoint startPoint, char open, char close, int maxLines, out SnapshotSpan pairSpan)
        {
            pairSpan = new SnapshotSpan(startPoint.Snapshot, 1, 1);
            var line = startPoint.GetContainingLine();
            var lineText = line.GetText();
            var lineNumber = line.LineNumber;
            var offset = startPoint.Position - line.Start.Position + 1;

            var stopLineNumber = startPoint.Snapshot.LineCount - 1;
            if (maxLines > 0)
                stopLineNumber = Math.Min(stopLineNumber, lineNumber + maxLines);

            var openCount = 0;
            while (true) {
                //walk the entire line
                while (offset < line.Length) {
                    var currentChar = lineText[offset];
                    if (currentChar == close) //found the close character
                    {
                        if (openCount > 0) {
                            openCount--;
                        } else //found the matching close
                        {
                            pairSpan = new SnapshotSpan(startPoint.Snapshot, line.Start + offset, 1);
                            return true;
                        }
                    } else if (currentChar == open) // this is another open
                    {
                        openCount++;
                    }
                    offset++;
                }

                //move on to the next line
                if (++lineNumber > stopLineNumber)
                    break;

                line = line.Snapshot.GetLineFromLineNumber(lineNumber);
                lineText = line.GetText();
                offset = 0;
            }

            return false;
        }

        private static bool FindMatchingOpenChar(SnapshotPoint startPoint, char open, char close, int maxLines, out SnapshotSpan pairSpan)
        {
            pairSpan = new SnapshotSpan(startPoint, startPoint);

            var line = startPoint.GetContainingLine();

            var lineNumber = line.LineNumber;
            var offset = startPoint - line.Start - 1; //move the offset to the character before this one

            //if the offset is negative, move to the previous line
            if (offset < 0) {
                line = line.Snapshot.GetLineFromLineNumber(--lineNumber);
                offset = line.Length - 1;
            }

            var lineText = line.GetText();

            var stopLineNumber = 0;
            if (maxLines > 0)
                stopLineNumber = Math.Max(stopLineNumber, lineNumber - maxLines);

            var closeCount = 0;

            while (true) {
                // Walk the entire line
                while (offset >= 0) {
                    var currentChar = lineText[offset];

                    if (currentChar == open) {
                        if (closeCount > 0) {
                            closeCount--;
                        } else // We've found the open character
                        {
                            pairSpan = new SnapshotSpan(line.Start + offset, 1); //we just want the character itself
                            return true;
                        }
                    } else if (currentChar == close) {
                        closeCount++;
                    }
                    offset--;
                }

                // Move to the previous line
                if (--lineNumber < stopLineNumber)
                    break;

                line = line.Snapshot.GetLineFromLineNumber(lineNumber);
                lineText = line.GetText();
                offset = line.Length - 1;
            }
            return false;
        }
    }
}