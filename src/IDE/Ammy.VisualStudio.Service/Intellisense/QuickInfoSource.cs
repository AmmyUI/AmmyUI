using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Ammy.Build;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Nitra;
using Nitra.Declarations;
using Ammy.Language;
using Ammy.VisualStudio.Service.Classifications;
using Ammy.VisualStudio.Service.Compilation;
using DotNet;
using Microsoft.VisualStudio.PlatformUI;
using System.Diagnostics;

namespace Ammy.VisualStudio.Service.Intellisense
{
    public class QuickInfoSource : IQuickInfoSource
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }
        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

        private readonly ITextBuffer _buffer;
        private readonly CompilerService _compilerService = CompilerService.Instance;
        private readonly ITextDocument _document;
        private static readonly SolidColorBrush AccentColor = new SolidColorBrush(Color.FromRgb(56, 140, 158));
        private static readonly SolidColorBrush LocationColor = new SolidColorBrush(Color.FromRgb(74, 140, 43));
        private static SolidColorBrush _normalTextBrush;
        private static bool _themeChangeHandlerAdded;

        public QuickInfoSource(ITextBuffer buffer)
        {
            _buffer = buffer;
            _buffer.Properties.TryGetProperty(typeof (ITextDocument), out _document);

            UpdateNormalTextBrush();

            if (!_themeChangeHandlerAdded) {
                VSColorTheme.ThemeChanged += args => {
                    UpdateNormalTextBrush();
                };
                _themeChangeHandlerAdded = true;
            }
        }

        private void UpdateNormalTextBrush()
        {
            var defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
            _normalTextBrush = new SolidColorBrush(Color.FromRgb(defaultForeground.R, defaultForeground.G, defaultForeground.B));
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            try {

                var subjectTriggerPoint = session.GetTriggerPoint(_buffer.CurrentSnapshot);
                if (!subjectTriggerPoint.HasValue) {
                    applicableToSpan = null;
                    return;
                }

                var currentSnapshot = subjectTriggerPoint.Value.Snapshot;

                //look for occurrences of our QuickInfo words in the span
                var navigator = NavigatorService.GetTextStructureNavigator(_buffer);
                var extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);
                var result = _compilerService.LatestResult?.GetFile(_document.FilePath);

                applicableToSpan = null;

                if (result == null)
                    return;

                bool foundSomething;
                SpanToSymbol(quickInfoContent, new NSpan(extent.Span.Start, extent.Span.End), result.Ast, out foundSomething);

                if (foundSomething)
                    applicableToSpan = currentSnapshot.CreateTrackingSpan(extent.Span.Start, extent.Span.Length, SpanTrackingMode.EdgeInclusive);
            } catch (Exception e) {
                Debug.WriteLine("AugmentQuickInfoSession failed");
                Debug.WriteLine(e.ToString());
            }
        }

        public static DeclarationSymbol SpanToSymbol(IList<object> quickInfoContent, NSpan span, IAst ast, out bool foundSomething)
        {
            var cts = new CancellationTokenSource();
            DeclarationSymbol symbol = null;
            var visitor = new CollectSymbolsAndRefsInSpanAstVisitor(cts.Token, span);
            visitor.Visit(ast);

            foreach (var rf in visitor.Refs) {
                if (rf.IsAmbiguous) {
                    var foundHint = false;
                    foreach (var ambiguity in rf.Ambiguities) {
                        var hint = SymbolToHint(ambiguity);
                        if (hint != null) {
                            quickInfoContent.Add(hint);
                            symbol = ambiguity;
                            foundHint = true;
                            break;
                        }
                    }

                    if (foundHint)
                        break;
                } else if (rf.IsSymbolEvaluated) {
                    var hint = SymbolToHint(rf.Symbol);
                    if (hint != null) {
                        quickInfoContent.Add(hint);
                        symbol = rf.Symbol;
                        break;
                    }
                }
            }

            foundSomething = visitor.Refs.Count > 0 || visitor.Names.Count > 0;

            return symbol;
        }

        private static object SymbolToHint(DeclarationSymbol symbol)
        {
            if (symbol == null)
                return "";

            var textBlock = new TextBlock();
            var inlines = textBlock.Inlines;

            var functionSymbol = symbol as FunctionSymbol;
            if (functionSymbol != null && functionSymbol.IsParametersEvaluated) {
                
                inlines.Add(GetSymbolType(symbol));
                var parameters = functionSymbol.Parameters.MakeCompletionList("");
                var parameterRuns = parameters.OfType<FunctionParameterSymbol>()
                                              .SelectMany(GetParameterHintString)
                                              .ToList();

                inlines.Add(CreateAccentRun(functionSymbol.Name));
                inlines.Add(CreateAccentRun("("));

                for (int i = 0; i < parameterRuns.Count; i++) {
                    inlines.Add(parameterRuns[i]);
                    if (i < parameterRuns.Count - 1)
                        inlines.Add(CreateRun(", "));
                }

                inlines.Add(CreateAccentRun(")"));
                inlines.AddRange(GetSymbolLocation(functionSymbol));
                return textBlock;
            }

            var variableSymbol = symbol as GlobalDeclaration.VariableSymbol;
            if (variableSymbol != null && variableSymbol.Value.HasValue) {
                var declaration = variableSymbol.FirstDeclarationOrDefault;

                if (declaration != null) {
                    inlines.Add(GetSymbolType(symbol));
                    inlines.Add(CreateRun(declaration.Location.GetText()));
                    inlines.AddRange(GetSymbolLocation(functionSymbol));
                    return textBlock;
                }

                return "";
            }

            if (symbol.IsFullNameEvaluated) {
                inlines.Add(GetSymbolType(symbol));
                inlines.Add(CreateRun(symbol.FullName));
                inlines.AddRange(GetSymbolLocation(symbol));
                return textBlock;
            }

            return null;
        }

        private static Run GetSymbolType(DeclarationSymbol symbol)
        {
            if (symbol is FunctionParameterSymbol)
                return CreateRun("(parameter) ");
            if (symbol is FunctionSymbol)
                return CreateRun("(mixin) ");
            if (symbol is TypeSymbol)
                return CreateRun("(type) ");
            if (symbol is Member.PropertySymbol)
                return CreateRun("(property) ");
            if (symbol is GlobalDeclaration.VariableSymbol)
                return CreateRun("(variable) ");
            return new Run();
        }

        private static Run CreateRun(string text)
        {
            return new Run(text) {
                Foreground = _normalTextBrush
            };
        }

        private static Run CreateAccentRun(string text)
        {
            return new Run(text) { Foreground = AccentColor };
        }

        private static IEnumerable<Run> GetSymbolLocation(DeclarationSymbol symbol)
        {
            if (symbol == null)
                return new Run[0];

            var declaration = symbol.FirstDeclarationOrDefault;
            var sourceSnapshot = declaration?.Source;
            var newLine = Environment.NewLine;

            if (sourceSnapshot != null && 
                sourceSnapshot != SourceSnapshot.Default && 
                sourceSnapshot.File != null && 
                declaration.Location != null) {

                var filename = Path.GetFileName(sourceSnapshot.File.FullName);
                var location = declaration.Location.StartLineColumn;

                return new [] {
                            new Run(newLine + filename + ":"+ location.Line + newLine + newLine) { Foreground = LocationColor }, 
                            CreateAccentRun("F12"),
                            CreateRun(" go to declaration "),
                            CreateAccentRun("(Ctrl + Click)")
                       };
            }

            return new Run[0];
        }

        private static IEnumerable<Run> GetParameterHintString(FunctionParameterSymbol p)
        {
            var declaration = (FunctionParameter) p.FirstDeclarationOrDefault;
            if (declaration.DefaultValue.HasValue) {
                var loc = declaration.DefaultValue.Location;
                var source = loc.Source;
                var defaultValueText = source.Text.Substring(loc.StartPos, loc.Length);

                return new [] {
                    CreateAccentRun(p.Name + " " + defaultValueText)
                };
            }

            return new[] { CreateAccentRun(p.Name) };
        }

        public void Dispose()
        {
        }
    }
}