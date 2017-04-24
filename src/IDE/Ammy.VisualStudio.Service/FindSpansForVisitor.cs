using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Nitra;
using Nitra.Declarations;

namespace Ammy.VisualStudio.Service
{
    class FindSpansForVisitor : IAstVisitor
    {
        public List<SpanInfo> SpanInfos { get; private set; }
        private readonly NSpan _span;

        public FindSpansForVisitor(SnapshotSpan span)
        {
            _span = new NSpan(span.Start, span.End);
            SpanInfos = new List<SpanInfo>();
        }
        
        public void Visit(IAst parseTree)
        {
            if (parseTree.Span.IntersectsWith(_span))
                parseTree.Accept(this);
        }

        public void Visit(Name name)
        {
            var span = name.Span;

            if (!span.IntersectsWith(_span) || !name.IsSymbolEvaluated)
                return;

            var sym = name.Symbol;
            var spanClass = sym.SpanClass;

            if (spanClass == Nitra.Language.DefaultSpanClass)
                return;

            SpanInfos.Add(new SpanInfo(span, spanClass));
        }

        public void Visit(Reference reference)
        {
            var span = reference.Span;

            if (!span.IntersectsWith(_span) || !reference.IsRefEvaluated)
                return;

            var spanClass = reference.Ref.SpanClass;

            if (spanClass == Nitra.Language.DefaultSpanClass)
                return;

            SpanInfos.Add(new SpanInfo(span, spanClass));
        }

        public void Visit(IRef r)
        {

            if(r.ResolvedTo != null)
            {
                Visit(r.ResolvedTo);
                return;
            }

            var spanClass = r.SpanClass;
            var span = r.Location.Span;

            if (spanClass == Nitra.Language.DefaultSpanClass)
                return;
            
            SpanInfos.Add(new SpanInfo(span, spanClass));
        }
    }
}