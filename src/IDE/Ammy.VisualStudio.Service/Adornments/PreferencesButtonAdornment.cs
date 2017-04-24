using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ammy.Build;
using Ammy.Language;
using Ammy.VisualStudio.Service.Views;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Image = System.Windows.Controls.Image;

namespace Ammy.VisualStudio.Service.Adornments
{
    class PreferencesButtonAdornment : INeedLogging, IDisposable
    {
        private readonly IWpfTextView _view;
        private readonly Button _button;
        private AmmyFile<Top> _recentFile;

        public PreferencesButtonAdornment(IAdornmentLayer adornmentLayer, IWpfTextView view)
        {
            _view = view;
            
            var buttonImage = new BitmapImage(new Uri("pack://application:,,,/Ammy.VisualStudio.Service;component/Resources/cog.png"));
            var factory = new FrameworkElementFactory(typeof(Image));

            factory.SetValue(Image.SourceProperty, buttonImage);

            _button = new Button {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(),
                Opacity = 0.5,
                Width = 20,
                Height = 20,
                Template = new ControlTemplate(typeof(Button)) {
                  VisualTree = factory
                }
            };

            _button.MouseEnter += ButtonOnMouseEnter;
            _button.MouseLeave += ButtonOnMouseLeave;
            _button.Click += ButtonOnClick;

            adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _button, null);

            _view.ViewportWidthChanged += ViewOnViewportWidthChanged;
            _button.Loaded += ButtonOnLoaded;
        }

        private void ButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var preferences = new Preferences(FormatDocument);
            preferences.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            preferences.ShowDialog();
            _view.VisualElement.Focus();
        }

        private void FormatDocument()
        {
            if (_recentFile != null) {
                var formatter = new Formatting("  ");
                var formattedText = formatter.FormatFile(_recentFile);
                var buffer = _view.TextBuffer;
                var textLength = buffer.CurrentSnapshot.Length;

                buffer.Replace(new Span(0, textLength), formattedText);
            }
        }

        private void ButtonOnMouseLeave(object sender, MouseEventArgs e)
        {
            _button.Opacity = 0.5;
        }

        private void ButtonOnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            _button.Opacity = 1;
        }

        private void ViewOnViewportWidthChanged(object sender, EventArgs eventArgs)
        {
            Canvas.SetLeft(_button, _view.ViewportWidth - _button.ActualWidth - 10);
            Canvas.SetTop(_button, 10);
        }

        private void ButtonOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Canvas.SetLeft(_button, _view.ViewportWidth - _button.ActualWidth - 10);
            Canvas.SetTop(_button, 10);

            _button.Loaded -= ButtonOnLoaded;
        }
        
        public void Update(AmmyFile<Top> file)
        {
            _recentFile = file;
        }

        public void Dispose()
        {
            _view.ViewportWidthChanged -= ViewOnViewportWidthChanged;
            _button.MouseEnter -= ButtonOnMouseEnter;
            _button.MouseLeave -= ButtonOnMouseLeave;
            _button.Click -= ButtonOnClick;
        }
    }
}