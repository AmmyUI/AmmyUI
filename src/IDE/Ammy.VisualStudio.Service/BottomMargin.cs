using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ammy.Language;
using EnvDTE;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Ammy.VisualStudio.Service.Classifications;
using Ammy.VisualStudio.Service.Compilation;
using Ammy.VisualStudio.Service.Errors;
using Ammy.VisualStudio.Service.Extensions;
using Ammy.VisualStudio.Service.Intellisense;
using Ammy.VisualStudio.Service.Taggers;
using Microsoft.VisualStudio.Text.Operations;
using Nitra;

namespace Ammy.VisualStudio.Service
{
    public class BottomMargin : Grid, IWpfTextViewMargin, INeedLogging
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        //private readonly IServiceProvider _serviceProvider;
        private readonly IWpfTextView _textView;
        private readonly ITextDocument _document;
        private readonly CompilerService _compilerService = CompilerService.Instance;
        private readonly CompilationIndicator _compilationIndicator;
        private readonly TextBlock _informationTextBlock;

        // ReSharper disable once UnusedParameter.Local
        public BottomMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer, IServiceProvider serviceProvider)
        {
            _textView = wpfTextViewHost.TextView;

            var properties = _textView.TextBuffer.Properties;
            properties.TryGetProperty(typeof (ITextDocument), out _document);
            
            _compilationIndicator = new CompilationIndicator(Children);
            _informationTextBlock = new TextBlock {
                Text = "",
                Foreground = Brushes.Green,
                Margin = new Thickness(15, 4, 15, 4)
            };

            Children.Add(_informationTextBlock);
            
            _textView.VisualElement.KeyDown += OnKeyDown;
            _textView.VisualElement.PreviewMouseDown += OnMouseDown;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl)) {
                var position = RelativeToView(_textView, e.GetPosition(_textView.VisualElement));
                var line = _textView.TextViewLines.GetTextViewLineContainingYCoordinate(position.Y);
                if (line == null)
                    return;

                var bufferPosition = line.GetBufferPositionFromXCoordinate(position.X);
                if (bufferPosition == null)
                    return;

                if (GoToDefinition(bufferPosition.Value))
                    e.Handled = true;
            }
        }

        private static Point RelativeToView(ITextView view, Point position)
        {
            return new Point(position.X + view.ViewportLeft, position.Y + view.ViewportTop);
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.F12)
                GoToDefinition(_textView.Caret.Position.BufferPosition);
        }

        private void ShowMessage(string message, SolidColorBrush foregroundBrush, SolidColorBrush backgroundBrush, int timeout = 15)
        {
            _informationTextBlock.Text = message;
            _informationTextBlock.Foreground = foregroundBrush;

            Observable.Timer(TimeSpan.FromSeconds(timeout), new SynchronizationContextScheduler(SynchronizationContext.Current))
                      .Subscribe(_ => {
                          _informationTextBlock.Text = string.Empty;
                          _informationTextBlock.Foreground = Brushes.Green;
                      });
        }

        public void Dispose()
        {
            _textView.VisualElement.KeyDown -= OnKeyDown;
            _textView.VisualElement.MouseDown -= OnMouseDown;

            _compilationIndicator?.Dispose();
        }

        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return marginName == "AmmyBottomMargin" ? this : null;
        }

        public bool GoToDefinition(int position)
        {
            var result = _compilerService.LatestResult?.GetFile(_document.FilePath);
            if (result == null)
                return false;

            var span = new NSpan(position, position);
            var quickInfoContent = new List<object>();
            bool foundSomething;
            var symbol = QuickInfoSource.SpanToSymbol(quickInfoContent, span, result.Ast, out foundSomething);
            var declaration = symbol?.FirstDeclarationOrDefault;
            var source = declaration?.Source;

            if (source != null && source != SourceSnapshot.Default)
                return Utils.MoveToLocation(declaration.Location);
            return false;
        }

        public double MarginSize => ActualHeight;
        public bool Enabled => true;
        public FrameworkElement VisualElement => this;
    }
}