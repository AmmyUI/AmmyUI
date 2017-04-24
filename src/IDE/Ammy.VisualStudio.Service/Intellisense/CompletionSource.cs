using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Nitra;
using Nitra.Declarations;
using Ammy.Infrastructure;
using Ammy.Language;
using Ammy.VisualStudio.Service.Compilation;
using Ammy.VisualStudio.Service.Extensions;
using DotNet;
using Nitra.Runtime.Reflection;
using Property = Ammy.Language.Property;

namespace Ammy.VisualStudio.Service.Intellisense
{
    internal class CompletionSource : ICompletionSource, INeedLogging
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }
        
        private readonly ITextBuffer _buffer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITextDocument _document;

        private readonly CompilerService _compilerService = CompilerService.Instance;
        
        private bool _isDisposed;

        public CompletionSource(ITextBuffer buffer, IServiceProvider serviceProvider)
        {
            _buffer = buffer;
            _serviceProvider = serviceProvider;
            _buffer.Properties.TryGetProperty(typeof (ITextDocument), out _document);
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            try {
                var currentSnapshot = _buffer.CurrentSnapshot;
                var file = _compilerService.LatestResult?.GetFile(_document.FilePath);
                var compiledSnapshot = file?.Meta.Snapshot as ITextSnapshot;

                if (compiledSnapshot == null || compiledSnapshot.TextBuffer != _buffer)
                    return;

                var span = FindTokenSpanAtPosition(session, currentSnapshot);
                var translatedSpan = span.TranslateTo(compiledSnapshot, SpanTrackingMode.EdgeExclusive);
                var startPoint = translatedSpan.End.Position < 0 ? 0 : translatedSpan.End.Position;
                var applicableTo = currentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive);
                var completions = CompleteWord(startPoint, file.Ast, file.ParseResult, file.GetSource(), compiledSnapshot);

                if (completions.Count > 0)
                    completionSets.Add(new MyCompletionSet("All", "All", applicableTo, completions, null, session));

            } catch (Exception e) {
                this.LogDebugInfo("AugmentCompletionSession failed: " + e);
            }
        }

        private HashSet<Completion> CompleteWord(int pos, IAst astRoot, IParseResult parseResult, SourceSnapshot source, ITextSnapshot compiledSnapshot)
        {
            var completionList = new HashSet<Completion>();
            var nspan = new NSpan(pos, pos);

            if (IsInsideComment(parseResult, nspan))
                return completionList;

            var visitor = new FindNodeAstVisitor(nspan);

            visitor.Visit(astRoot);
            var stack = visitor.Stack
                               .Where(ast => !(ast is IEnumerable)) // Skip IAstList
                               .ToArray();

            if (ShouldComplete(stack, pos, compiledSnapshot)) {
                GetCompletions(completionList, stack);
                AddKeywordCompletions(stack, completionList);
            }

            return completionList;
        }

        private static void AddKeywordCompletions(IAst[] stack, HashSet<Completion> completionList)
        {
            var els = stack.Zip(stack.Skip(1), (left, right) => new {left, right})
                .FirstOrDefault(a => !(a.left is Reference) && !(a.left is QualifiedReference));

            if (els != null) {
                var leftIsNodeType = els.left is Node || els.left is Function;

                if (leftIsNodeType && els.right is Top) {
                    completionList.Add(new MyCompletion("using", "using ", "", null, "", CompletionType.Normal, null, stack));
                    completionList.Add(new MyCompletion("mixin", "mixin ", "", null, "", CompletionType.Normal, null, stack));
                    completionList.Add(new MyCompletion("alias", "alias ", "", null, "", CompletionType.Normal, null, stack));
                }

                if (leftIsNodeType && !(els.right is Top)) {
                    completionList.Add(new MyCompletion("set", "set ", "", null, "", CompletionType.Normal, null, stack));
                }

                if (els.left is Property || leftIsNodeType) {
                    completionList.Add(new MyCompletion("combine", "combine ", "", null, "", CompletionType.Normal, null, stack));
                }

                if (els.left is PropertyValue) {
                    completionList.Add(new MyCompletion("resource", "resource ", "", null, "", CompletionType.Normal, null, stack));
                    completionList.Add(new MyCompletion("dyn", "dyn ", "", null, "", CompletionType.Normal, null, stack));
                    completionList.Add(new MyCompletion("true", "true", "", null, "", CompletionType.Normal, null, stack));
                    completionList.Add(new MyCompletion("false", "false", "", null, "", CompletionType.Normal, null, stack));
                    completionList.Add(new MyCompletion("null", "null", "", null, "", CompletionType.Normal, null, stack));
                }

                completionList.Add(new MyCompletion("bind", "bind ", "", null, "", CompletionType.Normal, null, stack));
                completionList.Add(new MyCompletion("convert", "convert ", "", null, "", CompletionType.Normal, null, stack));
                completionList.Add(new MyCompletion("from", "from ", "", null, "", CompletionType.Normal, null, stack));
            }
        }

        private static bool IsInsideComment(IParseResult parseResult, NSpan nspan)
        {
            var voidRuleWalker = new VoidRuleWalker(nspan);
            var spans = new HashSet<SpanInfo>();

            voidRuleWalker.Walk(parseResult, spans);

            foreach (var span in spans)
                if (span.Span.Contains(nspan) && span.SpanClass != Nitra.Language.DefaultSpanClass)
                    return true;
                
            return false;
        }

        private bool ShouldComplete(IAst[] stack, int pos, ITextSnapshot snapshot)
        {
            if (stack.Length == 0)
                return false;

            if (IsInsideBind(pos, snapshot))
                return false;

            var stackTop = stack[0];

            if (stackTop is Reference ||
                stackTop is QualifiedReference ||
                stackTop is Node ||
                stackTop is Function) {
                return true;
            }

            return false;
        }

        private static bool IsInsideBind(int pos, ITextSnapshot snapshot)
        {
            var currentLine = snapshot.GetLineFromPosition(pos);
            var currentLineText = currentLine.GetText().Substring(0, pos - currentLine.Start);

            if (Regex.IsMatch(currentLineText, @":\s*bind"))
                return true;

            if (currentLine.LineNumber > 0) {
                var previousLine = snapshot.GetLineFromLineNumber(currentLine.LineNumber - 1);
                var previousLineText = previousLine.GetText();

                if (Regex.IsMatch(previousLineText, @":\s*bind") && previousLineText.GetIndent().Length < currentLineText.GetIndent().Length)
                    return true;
            }

            return false;
        }

        private void GetCompletions(HashSet<Completion> completionList, IAst[] stack)
        {
            if (stack.Length == 0)
                return;

            var stackTop = stack[0];

            if (stackTop is Reference) {
                var reference = (Reference)stackTop;
                if (reference.IsScopeEvaluated)
                    ScopeToCompletions(completionList, reference.Scope, stack);
            }

            if (stackTop is NodeBase) {
                var nodeBase = (NodeBase)stackTop;
                if (nodeBase.Members.IsParentPropertyScopeEvaluated)
                    ScopeToCompletions(completionList, nodeBase.Members.ParentPropertyScope, stack);
            } else if (stackTop is NodeMember) {
                var member = (NodeMember)stackTop;
                if (member.IsParentPropertyScopeEvaluated)
                    ScopeToCompletions(completionList, member.ParentPropertyScope, stack);
            }
        }

        private void ScopeToCompletions(HashSet<Completion> completionList, Scope scope, IAst[] stack)
        {
            var symbols = scope.MakeCompletionList("");

            foreach (var symbol in symbols.Where(s => s != null))
                if (symbol.IsNameValid && !symbol.IsAbstract() && !IsSpecialSymbol(symbol))
                    completionList.Add(MyCompletion.FromSymbol(symbol, stack));
        }

        private bool IsSpecialSymbol(DeclarationSymbol symbol)
        {
            return symbol.Name.StartsWith("$");
        }

        private SnapshotSpan FindTokenSpanAtPosition(ICompletionSession session, ITextSnapshot snapshot)
        {
            var position = session.TextView.Caret.Position.BufferPosition.Position - 1;
            position = position >= 0 ? position : 0;

            var currentPoint = new SnapshotPoint(snapshot, position);

            var navigator = NavigatorService.GetTextStructureNavigator(_buffer);
            var extent = navigator.GetExtentOfWord(currentPoint);

            var span = extent.Span;
            if (extent.IsSignificant) {
                
                if (span.Start > 0) {
                    var prefixPoint = (span.Start - 1);
                    var prefix = prefixPoint.GetChar();

                    if (prefix == '@' || prefix == '$' || prefix == '#')
                        return new SnapshotSpan(prefixPoint, span.End);
                }
                return span;
            }

            return new SnapshotSpan(span.Snapshot, position, 0);
        }

        public void Dispose()
        {
            if (!_isDisposed) {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
    }

    public enum CompletionType { Property, Node, ContentFunctionRef, ContentFunctionRefWithParams, TypeFunctionRef, TypeFunctionRefWithParams, Normal }
}