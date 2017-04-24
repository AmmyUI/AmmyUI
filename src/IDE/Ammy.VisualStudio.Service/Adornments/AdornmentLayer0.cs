using System;
using System.Windows.Media;
using Ammy.Build;
using Ammy.Language;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Ammy.VisualStudio.Service.Adornments
{
    public class AdornmentLayer0 : INeedLogging, IDisposable
    {
        private readonly IAdornmentLayer _adornmentLayer;
        private readonly IWpfTextView _view;
        private readonly ColorMarkerAdornment _colorMarkerAdornment;
        private readonly ClosingBracketAdornment _closingBracketAdornment;
        private readonly PreferencesButtonAdornment _preferencesButtonAdornment;
        private readonly object _updatableAdornmentsTag = new object();

        private static SolidColorBrush _normalTextBrush;
        private static bool _themeChangeHandlerAdded;

        public AdornmentLayer0(IWpfTextView view, IAdornmentLayer adornmentAdornmentLayer)
        {
            _adornmentLayer = adornmentAdornmentLayer;
            _view = view;
            _view.LayoutChanged += OnLayoutChanged;
            _colorMarkerAdornment = new ColorMarkerAdornment(adornmentAdornmentLayer, view);
            _closingBracketAdornment = new ClosingBracketAdornment(adornmentAdornmentLayer, view);
            _preferencesButtonAdornment = new PreferencesButtonAdornment(adornmentAdornmentLayer, view);

            if (!_themeChangeHandlerAdded) {
                VSColorTheme.ThemeChanged += args => {
                    UpdateNormalTextBrush();
                };
                _themeChangeHandlerAdded = true;
            }

            UpdateNormalTextBrush();
        }

        private void UpdateNormalTextBrush()
        {
            var defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
            _normalTextBrush = new SolidColorBrush(Color.FromRgb(defaultForeground.R, defaultForeground.G, defaultForeground.B));
        }


        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            foreach (var line in e.NewOrReformattedLines)
                CreateVisuals(line);
        }
        
        private void CreateVisuals(ITextViewLine line)
        {
            _colorMarkerAdornment.CreateVisuals(line);
            _closingBracketAdornment.CreateVisuals(line, _normalTextBrush);
        }

        public void Update(AmmyFile<Top> file)
        {
            _colorMarkerAdornment.Update(file);
            _closingBracketAdornment.Update(file);
            _preferencesButtonAdornment.Update(file);

            _adornmentLayer.RemoveAdornmentsByTag(_colorMarkerAdornment);
            _adornmentLayer.RemoveAdornmentsByTag(_closingBracketAdornment);

            foreach (var line in _view.TextViewLines)
                CreateVisuals(line);
        }

        public void Dispose()
        {
            _preferencesButtonAdornment?.Dispose();
            _closingBracketAdornment?.Dispose();
        }
    }
}