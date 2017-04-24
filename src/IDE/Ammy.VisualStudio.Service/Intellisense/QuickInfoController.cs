using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Ammy.VisualStudio.Service.Intellisense
{
    public class IntellisenseController : IIntellisenseController
    {
        [Import]
        internal IQuickInfoBroker QuickInfoBroker { get; set; }

        private readonly IList<ITextBuffer> _buffers;
        private IQuickInfoSession _session;
        private ITextView _textView;

        public IntellisenseController(ITextView textView, IList<ITextBuffer> buffers)
        {
            _textView = textView;
            _buffers = buffers;

            _textView.MouseHover += OnTextViewMouseHover;
        }

        public void Detach(ITextView textView)
        {
            if (_textView == textView) {
                _textView.MouseHover -= OnTextViewMouseHover;
                _textView = null;
            }
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        private void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
        {
            //find the mouse position by mapping down to the subject buffer
            var snapshotPoint = new SnapshotPoint(_textView.TextSnapshot, e.Position);
            Predicate<ITextSnapshot> predicate = snapshot => _buffers.Contains(snapshot.TextBuffer);

            var point = _textView.BufferGraph.MapDownToFirstMatch(snapshotPoint, PointTrackingMode.Positive, predicate, PositionAffinity.Predecessor);
            
            if (point != null) {
                var triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position,
                    PointTrackingMode.Positive);

                if (!QuickInfoBroker.IsQuickInfoActive(_textView)) {
                    _session = QuickInfoBroker.TriggerQuickInfo(_textView, triggerPoint, true);
                }
            }
        }
    }
}