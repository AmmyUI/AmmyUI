using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Media;
using EnvDTE;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Nitra;
using Ammy.Build;
using Ammy.Language;
using Ammy.VisualStudio.Service.Compilation;

namespace Ammy.VisualStudio.Service.Classifications
{
    public class Classifier : IClassifier, INeedLogging
    {
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
        public static Brush CommonForegroundBrush;

        private readonly ITextBuffer _buffer;
        private readonly ITextDocument _document;
        private readonly IClassificationTypeRegistryService _classifications;
        private readonly IClassificationFormatMapService _classificationFormatMapService;
        private readonly IServiceProvider _serviceProvider;

        private static bool _customClassificationTypesAdded;
        private readonly CompilerService _compilerService = CompilerService.Instance;
        private readonly ClassificationSpan[] _emptyResult = new ClassificationSpan[0];
        private AmmyFile<Top> _latestFile;

        public Classifier(ITextBuffer buffer, ITextDocument document, IClassificationTypeRegistryService classifications, IClassificationFormatMapService classificationFormatMapService, IServiceProvider serviceProvider)
        {
            _buffer = buffer;
            _document = document;
            _classifications = classifications;
            _classificationFormatMapService = classificationFormatMapService;
            _serviceProvider = serviceProvider;

            TaskManager.Initialize(_serviceProvider);

            AddDotNetClassificationTypes();
        }

        public void Update(AmmyFile<Top> file)
        {
            _latestFile = file;

            var snapshot = (ITextSnapshot)_latestFile.Meta.Snapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, 0, snapshot.Length);
            var eventArgs = new ClassificationChangedEventArgs(snapshotSpan);

            ClassificationChanged?.Invoke(this, eventArgs);
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            try {
                if (_latestFile == null)
                    return new List<ClassificationSpan>();

                var compiledSnapshot = _latestFile.Meta.Snapshot as ITextSnapshot;

                if (compiledSnapshot == null || compiledSnapshot.TextBuffer != _buffer)
                    return _emptyResult;

                return GetSpansFor(compiledSnapshot, span, _latestFile);
            } catch (Exception e) {
                this.LogDebugInfo("GetClassificationSpans failed: " + e);
                return _emptyResult;
            }
        }

        private IList<ClassificationSpan> GetSpansFor(ITextSnapshot compiledSnapshot, SnapshotSpan span, AmmyFile<Top> file)
        {
            var spanSnapshot = span.Snapshot;
            var result = new List<ClassificationSpan>();
            var ast = file.Ast;
            var translatedSpan = span.TranslateTo(compiledSnapshot, SpanTrackingMode.EdgeExclusive);

            var spanInfos = new HashSet<SpanInfo>();
            file.ParseResult.GetSpans(span.Start, span.End, spanInfos);

            var spanFinder = new FindSpansForVisitor(translatedSpan);
            ast.Accept(spanFinder);

            var allSpans = spanFinder.SpanInfos
                                     .Concat(spanInfos)
                                     .Where(si => si.Span.Length != spanSnapshot.Length)
                                     .Distinct();

            foreach (var spanInfo in allSpans) {
                var astSpan = spanInfo.Span;
                var newSpan = new SnapshotSpan(compiledSnapshot, new Span(astSpan.StartPos, astSpan.Length))
                                    .TranslateTo(spanSnapshot, SpanTrackingMode.EdgeInclusive);
                var classificationType = _classifications.GetClassificationType(TranslateSpanName(spanInfo.SpanClass.FullName));

                if (!newSpan.IntersectsWith(span))
                    continue;

                if (newSpan.Start > 0) {
                    var prefix = spanSnapshot.GetText(newSpan.Start - 1, 1);
                    if (prefix == "@" || prefix == "#" || prefix == "$")
                        newSpan = new SnapshotSpan(newSpan.Snapshot, newSpan.Start - 1, newSpan.Length + 1);
                }

                result.Add(new ClassificationSpan(newSpan, classificationType));
            }

            return result;
        }

        private static string TranslateSpanName(string spanName)
        {
            switch (spanName) {
                case "Nitra.Language.Keyword": return "Ammy Keyword";
                case "Nitra.Language.Type": return "Ammy Type";
                case "Nitra.Language.String": return "Ammy Value";
                case "Nitra.Language.Number": return "Ammy Value";
                case "Nitra.Language.Operator": return "Ammy Variable";
                case "DotNetLang.Method": return "Ammy Mixin";
                case "DotNetLang.Namespace": return "Ammy Type";
                case "DotNetLang.Parameter": return "Ammy Variable";
                case "DotNetLang.Property": return "Ammy Property";
                case "DotNetLang.Field": return "Ammy Property";
                default: return spanName;
            }
        }

        private void AddDotNetClassificationTypes()
        {
            if (!_customClassificationTypesAdded) {
                _customClassificationTypesAdded = true;
                
                var identiferClassificationType = _classifications.GetClassificationType("identifier");
                var classificationFormatMap = _classificationFormatMapService.GetClassificationFormatMap(category: "text");
                var spanClasses = DotNetLang.Instance.GetSpanClasses();
                var identifierProperties = classificationFormatMap.GetExplicitTextProperties(identiferClassificationType);

                foreach (var spanClass in spanClasses) {
                    if (_classifications.GetClassificationType(spanClass.FullName) != null) 
                        continue;
                    
                    var classificationType = _classifications.CreateClassificationType(spanClass.FullName, new[] { identiferClassificationType });
                    var argb = spanClass.Style.ForegroundColor;
                    var color = Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
                    var classificationTypeProperties = identifierProperties.SetForeground(color);

                    classificationFormatMap.AddExplicitTextProperties(classificationType, classificationTypeProperties);
                }

                var textClassificationType = _classifications.GetClassificationType("text");
                var textProperties = classificationFormatMap.GetExplicitTextProperties(textClassificationType);

                //CommonForegroundBrush = textProperties.ForegroundBrush;
            }
        }
    }
    
    public enum VsTheme
    {
        Unknown = 0,
        Light,
        Dark,
        Blue
    }
}