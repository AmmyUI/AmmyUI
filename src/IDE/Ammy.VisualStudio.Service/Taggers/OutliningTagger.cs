using System;
using System.Collections.Generic;
using System.Linq;
using Ammy.Build;
using Ammy.Language;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Nitra;

namespace Ammy.VisualStudio.Service.Taggers
{
    public class OutliningTagger : ITagger<IOutliningRegionTag>
    {
        private readonly List<ITagSpan<IOutliningRegionTag>> _regionList = new List<ITagSpan<IOutliningRegionTag>>();
        private readonly ITextBuffer _buffer;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public OutliningTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
        }

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            return _regionList;
        }

        public void Update(AmmyFile<Top> file)
        {
            _regionList.Clear();

            var snapshot = (ITextSnapshot)file.Meta.Snapshot;
            var nodeCollector = new AstCollectorVisitor(ast => ast is Node || ast is Function || ast is TypeFunctionRef);

            file.Ast.Accept(nodeCollector);

            foreach (var item in nodeCollector.CollectedItems) {
                var location = item.Location;

                if (location.StartLineColumn.Line == location.EndLineColumn.Line)
                    continue;

                var start = location.StartPos;
                var text = location.GetText();
                var closingBracketPos = text.LastIndexOf('}');
                var openingBraceIndex = text.IndexOf("{", StringComparison.InvariantCultureIgnoreCase);

                var ellipsis = openingBraceIndex > -1
                               ? text.Substring(0, openingBraceIndex)
                               : text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).FirstOrDefault();

                var snapshotSpan = new SnapshotSpan(snapshot, start, closingBracketPos + 1);
                var outliningRegionTag = new OutliningRegionTag(false, false, ellipsis, text);
                var span = new TagSpan<IOutliningRegionTag>(snapshotSpan, outliningRegionTag);

                _regionList.Add(span);
            }

            var args = new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length));
            TagsChanged?.Invoke(this, args);
        }
    }
}