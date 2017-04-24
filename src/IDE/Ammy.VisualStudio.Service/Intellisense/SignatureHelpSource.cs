using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ammy.Language;
using Ammy.VisualStudio.Service.Compilation;
using EnvDTE;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Nitra;
using Nitra.Declarations;
using Nitra.Runtime.Reflection;

namespace Ammy.VisualStudio.Service.Intellisense
{
    internal class SignatureHelpSource : ISignatureHelpSource
    {
        private readonly CompilerService _compilerService = CompilerService.Instance;
        private readonly ITextBuffer _buffer;
        private readonly ITextDocument _document;
        private bool _isDisposed;

        public SignatureHelpSource(ITextBuffer buffer)
        {
            _buffer = buffer;
            _buffer.Properties.TryGetProperty(typeof(ITextDocument), out _document);
        }

        public void Dispose()
        {
            if (!_isDisposed) {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures)
        {
            var snapshot = _buffer.CurrentSnapshot;
            var trackingPoint = session.GetTriggerPoint(_buffer);
            var position = trackingPoint.GetPosition(snapshot);
            var file = _compilerService.LatestResult?.GetFile(_document.FilePath);
            var compiledSnapshot = file?.Meta.Snapshot as ITextSnapshot;

            if (compiledSnapshot == null || compiledSnapshot.TextBuffer != _buffer)
                return;

            var span = new Span(position, 0);
            var applicableToSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive, 0);
            var signature = GetSignature(position, file.Ast, applicableToSpan);

            if (signature != null)
                signatures.Add(signature);
        }

        public ISignature GetBestMatch(ISignatureHelpSession session)
        {
            if (session.Signatures.Count > 0)
                return session.Signatures[0];
            return null;
        }

        private MixinSignature CreateSignature(ITextBuffer textBuffer, string methodSig, string methodDoc, ITrackingSpan span)
        {
            var sig = new MixinSignature(textBuffer, methodSig, methodDoc, null);
            textBuffer.Changed += sig.OnSubjectBufferChanged;

            //find the parameters in the method signature (expect methodname(one, two)
            var pars = methodSig.Split('(', ',', ')');
            var paramList = new List<IParameter>();

            var locusSearchStart = 0;
            for (var i = 1; i < pars.Length; i++) {
                var param = pars[i].Trim();

                if (string.IsNullOrEmpty(param))
                    continue;

                //find where this parameter is located in the method signature
                var locusStart = methodSig.IndexOf(param, locusSearchStart);
                if (locusStart >= 0) {
                    var locus = new Span(locusStart, param.Length);
                    locusSearchStart = locusStart + param.Length;
                    paramList.Add(new MixinParameter(null, locus, param, sig));
                }
            }

            sig.Parameters = new ReadOnlyCollection<IParameter>(paramList);
            sig.ApplicableToSpan = span;
            sig.ComputeCurrentParameter();

            return sig;
        }

        private MixinSignature GetSignature(int pos, IAst astRoot, ITrackingSpan applicableToSpan)
        {
            var visitor = new FindNodeAstVisitor(new NSpan(pos, pos));
            visitor.Visit(astRoot);

            foreach (var ast in visitor.Stack) {
                if (ast is FunctionRef) {
                    var functionRef = (FunctionRef) ast;
                    if (functionRef.IsSymbolEvaluated && functionRef.Symbol.IsFunctionEvaluated) {
                        var symbol = functionRef.Symbol;
                        var function = symbol.Function;
                        var parameters = function.Parameters.MakeCompletionList("")
                                                 .OfType<FunctionParameterSymbol>()
                                                 .OrderBy(p => p.Index)
                                                 .Select(p => p.Name)
                                                 .ToList();
                        var paramString = string.Join(", ", parameters);

                        return CreateSignature(_buffer, function.Name + "(" + paramString + ")", "", applicableToSpan);
                    }

                    return null;
                }
            }

            return null;
        }
    }
}