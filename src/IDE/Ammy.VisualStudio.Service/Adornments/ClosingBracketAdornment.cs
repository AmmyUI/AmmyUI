using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ammy.Build;
using Ammy.Infrastructure;
using Ammy.Language;
using Ammy.VisualStudio.Service.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Ammy.VisualStudio.Service.Adornments
{
    class ClosingBracketAdornment : INeedLogging, IDisposable
    {
        private readonly IWpfTextView _view;
        private readonly IAdornmentLayer _adornmentLayer;
        private readonly IList<ClosingBracketSpan> _currentSpans = new List<ClosingBracketSpan>();
        private SolidColorBrush _currentBrush;

        public ClosingBracketAdornment(IAdornmentLayer adornmentLayer, IWpfTextView view)
        {
            _adornmentLayer = adornmentLayer;
            _view = view;

            AmmySettings.SettingsChanged += AmmySettingsOnSettingsChanged;
        }

        private void AmmySettingsOnSettingsChanged(object sender, EventArgs eventArgs)
        {
            _adornmentLayer.RemoveAdornmentsByTag(this);

            foreach (var line in _view.TextViewLines)
                CreateVisuals(line, _currentBrush);
        }

        public void CreateVisuals(ITextViewLine line, SolidColorBrush normalTextBrush)
        {
            if (!AmmySettings.ShowEndTagAdornments)
                return;

            _currentBrush = normalTextBrush;

            var textViewLines = _view.TextViewLines;
            var addedBounds = new List<Rect>();

            foreach (var span in _currentSpans) {
                var compiledSnapshotSpan = span.SnapshotSpan.TranslateTo(_view.TextSnapshot, SpanTrackingMode.EdgeExclusive);
                
                if (compiledSnapshotSpan.IntersectsWith(line.Extent)) {
                    var geometry = textViewLines.GetMarkerGeometry(compiledSnapshotSpan);
                    if (geometry != null) {
                        var bounds = geometry.Bounds;

                        if (addedBounds.Any(rect => rect.IntersectsWith(bounds)))
                            continue;

                        addedBounds.Add(bounds);

                        var textBlock = new TextBlock {
                            Text = span.FullName,
                            FontSize = 10,
                            Opacity = 0.3,
                            Foreground = normalTextBrush
                        };

                        Canvas.SetLeft(textBlock, bounds.Right + 3);
                        Canvas.SetTop(textBlock, line.TextTop - 0.5);

                        _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, compiledSnapshotSpan, this, textBlock, null);
                    }
                }
            }
        }

        public void Update(AmmyFile<Top> file)
        {
            try {
                _currentSpans.Clear();

                var fileSnapshot = (ITextSnapshot)file.Meta.Snapshot;
                var collector = new AstCollectorVisitor(ast => ast is Node || ast is TypeFunctionRef);

                file.Ast.Accept(collector);

                if (file.GetErrors().Any())
                    return;

                foreach (var item in collector.CollectedItems) {
                    if (item.Location.EndLineColumn.Line - item.Location.StartLineColumn.Line < 5)
                        continue;

                    if (item is Node) {
                        var node = (Node)item;
                        var fullName = "end of " + node.Key.FullName();

                        if (node.NodeName.HasValue && node.NodeName.Value.Key.HasValue)
                            fullName += $" \"{node.NodeName.Value.Key.Value}\"";

                        var text = node.Location.GetText();
                        var closingBracketPos = text.LastIndexOf('}');
                        var position = node.Location.StartPos + closingBracketPos;

                        _currentSpans.Add(new ClosingBracketSpan(fullName, new SnapshotSpan(fileSnapshot, position, 1)));
                    } else if (item is TypeFunctionRef) {
                        var tfr = (TypeFunctionRef)item;
                        var fullName = "end of @" + tfr.FunctionRef.Name;
                        var text = tfr.Location.GetText();
                        var closingBracketPos = text.LastIndexOf('}');
                        var position = tfr.Location.StartPos + closingBracketPos;

                        _currentSpans.Add(new ClosingBracketSpan(fullName, new SnapshotSpan(fileSnapshot, position, 1)));
                    }
                }
            } catch (Exception e) {
                this.LogDebugInfo(e.ToString());
            }
        }

        class ClosingBracketSpan
        {
            public SnapshotSpan SnapshotSpan { get; }
            public string FullName { get; }

            public ClosingBracketSpan(string fullName, SnapshotSpan snapshotSpan)
            {
                FullName = fullName;
                SnapshotSpan = snapshotSpan;
            }
        }

        public void Dispose()
        {
            AmmySettings.SettingsChanged -= AmmySettingsOnSettingsChanged;
        }
    }
}