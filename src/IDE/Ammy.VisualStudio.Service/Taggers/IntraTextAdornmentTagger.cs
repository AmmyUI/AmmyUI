using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Ammy.Build;
using Ammy.Language;
using Ammy.VisualStudio.Service.Compilation;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Ammy.VisualStudio.Service.Taggers
{
    public class IntraTextAdornmentTagger : ITagger<IntraTextAdornmentTag>
    {
        private readonly ITextBuffer _buffer;
        private readonly CompilerService _compilerService = CompilerService.Instance;
        private readonly ITextDocument _document;
        
        public IntraTextAdornmentTagger(ITextBuffer buffer)
        {
            _buffer = buffer;

            buffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out _document);
        }

        public IEnumerable<ITagSpan<IntraTextAdornmentTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            return new ITagSpan<IntraTextAdornmentTag>[0];
            //foreach (var span in spans) {
            //    foreach (var currentSpan in _currentSpans) {
            //        var snapshotSpan = currentSpan.SnapshotSpan;
            //        if (span.OverlapsWith(snapshotSpan)) {
            //            yield return new TagSpan<IntraTextAdornmentTag>(snapshotSpan, new IntraTextAdornmentTag(CreateAdornment(currentSpan), null, PositionAffinity.Successor));
            //        }
            //    }
            //}
        }

        public void Update(CompileResult result)
        {
            return;
            /*
            var file = result.GetFile(_document.FilePath);
            if (file == null)
                return;
            
            _currentSpans.Clear();

            var fileSnapshot = (ITextSnapshot)file.Meta.Snapshot;
            var collector = new AstCollectorVisitor(ast => ast is PropertyValue.String || ast is PropertyValue.ReferenceValue);

            file.Ast.Accept(collector);

            foreach (var item in collector.CollectedItems) {
                if (item is PropertyValue.String) {
                    var pvString = (PropertyValue.String) item;
                    if (pvString.Val.HasValue) {
                        var stringValue = pvString.Val.Value;
                        var match = Regex.Match(stringValue, @"#([0-9a-fA-F]{8})|#((?:[0-9a-fA-F]{3}){1,2})");
                        if (match.Success) {
                            var brush = ParseColorValue(match.Value);
                            _currentSpans.Add(new ColorValueSpan(brush, new SnapshotSpan(fileSnapshot, item.Location.StartPos, item.Location.Length)));
                        }
                    }
                } else if (item is PropertyValue.ReferenceValue) {
                    
                }
            }

            RaiseTagsChanged(new SnapshotSpanEventArgs(new SnapshotSpan(_buffer.CurrentSnapshot, 0, _buffer.CurrentSnapshot.Length)));
            */
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public void RaiseTagsChanged(SnapshotSpanEventArgs snapshotSpanEventArgs)
        {
            TagsChanged?.Invoke(this, snapshotSpanEventArgs);
        }
    }
}