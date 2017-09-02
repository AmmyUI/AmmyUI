using System;
using System.Collections.Generic;
using Ammy.VisualStudio.Service.Infrastructure;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using System.Diagnostics;
using System.Linq;

namespace Ammy.VisualStudio.Service.Taggers
{
    public class TextMarkerTagger : ITagger<TextMarkerTag>
    {
        private readonly BraceMatching _braceMatching;
        private readonly ITextBuffer _buffer;

        public TextMarkerTagger(IServiceProvider serviceProvider, ITextBuffer buffer, ITextView textView)
        {
            _buffer = buffer;
            _braceMatching = new BraceMatching(this, buffer, textView);
        }

        public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            try {
                return _braceMatching.GetTags(spans);
            } catch (Exception e) {
                Debug.WriteLine("GetTags failed");
                Debug.WriteLine(e.ToString());

                return Enumerable.Empty<ITagSpan<TextMarkerTag>>();
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public void RaiseTagsChanged(SnapshotSpanEventArgs snapshotSpanEventArgs)
        {
            TagsChanged?.Invoke(this, snapshotSpanEventArgs);
        }
    }
}