using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ammy.Build;
using Ammy.Language;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Ammy.VisualStudio.Service.Adornments
{
    class ColorMarkerAdornment : INeedLogging
    {
        private readonly IWpfTextView _view;
        private readonly IAdornmentLayer _adornmentLayer;
        private readonly IList<ColorValueSpan> _currentSpans = new List<ColorValueSpan>();
        private readonly Pen _pen = new Pen(Brushes.Transparent, 0);
        private readonly Type _brushesType;
        private readonly PropertyInfo[] _brushProperties;
        private readonly Dictionary<string, Brush> _cachedBrushes = new Dictionary<string, Brush>();

        public ColorMarkerAdornment(IAdornmentLayer adornmentLayer, IWpfTextView view)
        {
            _adornmentLayer = adornmentLayer;
            _view = view;
            _brushesType = typeof(Brushes);
            _brushProperties = _brushesType.GetProperties(BindingFlags.Public | BindingFlags.Static);
        }

        public void CreateVisuals(ITextViewLine line)
        {
            var textViewLines = _view.TextViewLines;

            foreach (var span in _currentSpans) {
                var compiledSnapshotSpan = span.SnapshotSpan.TranslateTo(_view.TextSnapshot, SpanTrackingMode.EdgeExclusive);

                if (compiledSnapshotSpan.IntersectsWith(line.Extent)) {
                    var geometry = textViewLines.GetMarkerGeometry(compiledSnapshotSpan);
                    if (geometry != null) {
                        var bounds = geometry.Bounds;
                        var newGeometry = new RectangleGeometry(new Rect(new Point(bounds.BottomLeft.X, bounds.BottomLeft.Y - 3.0), new Point(bounds.BottomRight.X, bounds.BottomRight.Y)));
                        var newGeometryBounds = newGeometry.Bounds;
                        var drawing = new GeometryDrawing(span.Brush, _pen, newGeometry);
                        drawing.Freeze();

                        var drawingImage = new DrawingImage(drawing);
                        drawingImage.Freeze();

                        var image = new Image {
                            Source = drawingImage
                        };

                        // Align the image with the top of the bounds of the text geometry
                        Canvas.SetLeft(image, newGeometryBounds.Left);
                        Canvas.SetTop(image, newGeometryBounds.Top);

                        _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, compiledSnapshotSpan, this, image, null);
                    }
                }
            }
        }

        public void Update(AmmyFile<Top> file)
        {
            try {
                _currentSpans.Clear();

                var fileSnapshot = (ITextSnapshot)file.Meta.Snapshot;
                var collector = new AstCollectorVisitor(ast => ast is PropertyValue.String || ast is PropertyValue.ReferenceValue);

                file.Ast.Accept(collector);

                foreach (var item in collector.CollectedItems) {
                    if (item is PropertyValue.String) {
                        var pvString = (PropertyValue.String)item;
                        if (pvString.Val.HasValue) {
                            var stringValue = pvString.Val.Value;
                            var match = Regex.Match(stringValue, @"^#([0-9a-fA-F]{8})$|^#((?:[0-9a-fA-F]{3}){1,2})$");
                            if (match.Success) {
                                var brush = ParseColorValue(match.Value);
                                _currentSpans.Add(new ColorValueSpan(brush, new SnapshotSpan(fileSnapshot, item.Location.StartPos, item.Location.Length)));
                            }
                        }
                    } else if (item is PropertyValue.ReferenceValue) {
                        var rvalue = (PropertyValue.ReferenceValue)item;
                        if (rvalue.IsRefEvaluated && rvalue.Ref.IsSymbolEvaluated) {
                            var symbol = rvalue.Ref.Symbol;

                            if (_brushesType.Namespace == null)
                                continue;

                            if (symbol.FullName.StartsWith(_brushesType.Namespace)) {
                                foreach (var brushProperty in _brushProperties) {
                                    if (brushProperty.Name == symbol.Name) {
                                        var brush = GetPredefinedBrush(brushProperty);
                                        var location = rvalue.Ref.Location;

                                        _currentSpans.Add(new ColorValueSpan(brush, new SnapshotSpan(fileSnapshot, location.StartPos, location.Length)));
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception e) {
                this.LogDebugInfo(e.ToString());
            }
        }

        private Brush ParseColorValue(string hex)
        {
            var brush = (Brush)new BrushConverter().ConvertFromInvariantString(hex);

            if (brush != null)
                brush.Freeze();

            return brush;
        }

        private Brush GetPredefinedBrush(PropertyInfo brushProperty)
        {
            Brush brush;
            if (_cachedBrushes.TryGetValue(brushProperty.Name, out brush))
                return brush;

            _cachedBrushes[brushProperty.Name] = brush = (Brush)brushProperty.GetValue(null);

            return brush;
        }

        class ColorValueSpan
        {
            public SnapshotSpan SnapshotSpan { get; }
            public Brush Brush { get; }

            public ColorValueSpan(Brush brush, SnapshotSpan snapshotSpan)
            {
                Brush = brush;
                SnapshotSpan = snapshotSpan;
            }
        }
    }
}