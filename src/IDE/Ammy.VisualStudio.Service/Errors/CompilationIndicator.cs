using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Ammy.Build;
using Ammy.Language;
using Ammy.VisualStudio.Service.Compilation;
using Ammy.VisualStudio.Service.Extensions;
using DotNet;
using EnvDTE;
using Nitra.ProjectSystem;

namespace Ammy.VisualStudio.Service.Errors
{
    class CompilationIndicator : IDisposable, INeedLogging
    {
        public Ellipse Control { get; }

        private static readonly Lazy<ErrorIndicatorHelper> LazyHelper = new Lazy<ErrorIndicatorHelper>(() => new ErrorIndicatorHelper());
        private ErrorIndicatorHelper _helper;
        private readonly SolidColorBrush _successColor = Brushes.Green;
        private readonly SolidColorBrush _failColor = Brushes.Red;
        private readonly SolidColorBrush _compilingColor = Brushes.DodgerBlue;

        public CompilationIndicator(UIElementCollection host)
        {
            Control = new Ellipse {
                Fill = _successColor,
                Width = 18,
                Height = 18,
                Margin = new Thickness(3),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Control.MouseDown += ControlOnMouseDown;

            host.Add(Control);

            Control.Loaded += Initialize;
        }

        private void Initialize(object sender, RoutedEventArgs e)
        {
            _helper = LazyHelper.Value;

            _helper.Success += ValueOnSuccess;
            _helper.Fail += ValueOnFail;
            _helper.Compiling += Compiling;
        }

        private void Compiling(object sender, EventArgs e)
        {
            //Control.Fill = _compilingColor;
        }

        private void ValueOnFail(object sender, EventArgs eventArgs)
        {
            Control.Fill = _failColor;
        }

        private void ValueOnSuccess(object sender, EventArgs eventArgs)
        {
            Control.Fill = _successColor;
        }

        private void ControlOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            _helper?.IndicatorClicked();
        }

        public void Dispose()
        {
            Control.Loaded -= Initialize;

            var helper = _helper;

            if (helper != null) {
                helper.Success -= ValueOnSuccess;
                helper.Fail -= ValueOnFail;
                helper.Compiling -= Compiling;
            }
        }
    }

    class ErrorIndicatorHelper : INeedLogging
    {
        private readonly List<CompilerMessage> _errors = new List<CompilerMessage>();
        private readonly CompilerService _compilerService = CompilerService.Instance;

        public event EventHandler Success = (sender, args) => { };
        public event EventHandler Fail = (sender, args) => { };
        public event EventHandler Compiling = (sender, args) => { };

        private int _currentErrorIndex;

        public ErrorIndicatorHelper()
        {
            _compilerService.Compilations
                            .ObserveOn(SynchronizationContext.Current)
                            .Subscribe(ProcessCompileResult);

            _compilerService.IsCompiling 
                            .DistinctUntilChanged()
                            .ObserveOn(SynchronizationContext.Current)
                            .Subscribe(isCompiling => {
                                if (isCompiling)
                                    Compiling(this, new EventArgs());
                            });
        }

        private void ProcessCompileResult(CompileResult result)
        {
            TaskManager.ClearMessages();

            if (result.IsSuccess) {
                Success(this, new EventArgs());
            } else {
                Fail(this, new EventArgs());

                _errors.Clear();
                _currentErrorIndex = 0;

                foreach (var file in result.Files)
                foreach (var error in file.GetErrors()) {
                    _errors.Add(error);
                        
                    var location = error.Location;
                    var filename = location.Source?.File?.FullName ?? "";
                    var lineColumn = location.StartLineColumn;

                    TaskManager.AddError(error.Text, filename, lineColumn.Line, lineColumn.Column);
                }
            }
        }

        public void IndicatorClicked()
        {
            try {
                if (_errors.Count == 0) return;

                if (_currentErrorIndex < _errors.Count) {
                    var currentError = _errors[_currentErrorIndex++];

                    Utils.MoveToLocation(currentError.Location);

                    if (_currentErrorIndex >= _errors.Count)
                        _currentErrorIndex = 0;
                }
            } catch (Exception e) {
                this.LogDebugInfo("CompilationIndicator click exception: " + e);
            }
        }
    }
}
