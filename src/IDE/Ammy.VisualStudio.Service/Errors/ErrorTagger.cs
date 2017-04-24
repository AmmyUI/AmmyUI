using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using Nitra;
using Nitra.ProjectSystem;
using Ammy.Build;
using Ammy.Language;

namespace Ammy.VisualStudio.Service.Errors
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ErrorTagger : ITagger<ErrorTag>, INeedLogging
    {
        private readonly ITextBuffer _buffer;
        private readonly IEnumerable<ITagSpan<ErrorTag>> _emptyTags = Enumerable.Empty<ITagSpan<ErrorTag>>();
        private AmmyFile<Top> _latestFile;

        // ReSharper disable once UnusedParameter.Local
        public ErrorTagger(ITextBuffer buffer, IServiceProvider serviceProvider)
        {
            _buffer = buffer;
        }

        public void Update(AmmyFile<Top> file)
        {
            _latestFile = file;

            var snapshot = (ITextSnapshot)_latestFile.Meta.Snapshot;
            var args = new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length));

            TagsChanged?.Invoke(this, args);
        }

        public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            try {
                var snapshot = _buffer.CurrentSnapshot;

                if (_latestFile == null)
                    return _emptyTags;
            
                var localMessages = _latestFile.GetErrors();

                return localMessages.Where(msg => msg.Location.EndPos <= snapshot.Length)
                                    .Select(msg => TagSpanFromMessage(msg, snapshot));
            } catch (Exception e) {
                this.LogDebugInfo("ErrorTagger.GetTags failed: " + e);
                return new ITagSpan<ErrorTag>[0];
            }
        }

        private TagSpan<ErrorTag> TagSpanFromMessage(CompilerMessage msg, ITextSnapshot snapshot)
        {
            var span = new Span(msg.Location.StartPos, msg.Location.Length);
            var snapshotSpan = new SnapshotSpan(snapshot, span);
            var errorTag = new ErrorTag(TranslateErrorType(msg.Type), msg.Text);

            return new TagSpan<ErrorTag>(snapshotSpan, errorTag);
        }

        private string TranslateErrorType(CompilerMessageType type)
        {
            if (type == CompilerMessageType.Warning)
                return PredefinedErrorTypeNames.Warning;
            
            return PredefinedErrorTypeNames.SyntaxError;
        }
        
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}