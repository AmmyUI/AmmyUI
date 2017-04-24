using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Ammy.Build;
using Ammy.Language;
using Ammy.VisualStudio.Service.Adornments;
using Ammy.VisualStudio.Service.Classifications;
using Ammy.VisualStudio.Service.Compilation;
using Ammy.VisualStudio.Service.Errors;
using Ammy.VisualStudio.Service.Extensions;
using Ammy.VisualStudio.Service.Taggers;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Ammy.VisualStudio.Service
{
    public class WpfTextViewHandler : ICompilationListener
    {
        private readonly IWpfTextView _textView;
        private readonly CompilerService _compilerService;
        private readonly CompositeDisposable _disposer = new CompositeDisposable();
        private readonly ITextDocument _document;
        private readonly AdornmentLayer0 _layer0;

        public WpfTextViewHandler(IWpfTextView textView)
        {
            _textView = textView;
            _compilerService = CompilerService.Instance;

            textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out _document);

            if (_document != null) {
                FilePath = _document.FilePath;

                var bufferChanged = Observable.FromEventPattern<TextContentChangedEventArgs>(s => _textView.TextBuffer.Changed += s,
                                                                                             s => _textView.TextBuffer.Changed -= s);

                var fileAction = Observable.FromEventPattern<TextDocumentFileActionEventArgs>(s => _document.FileActionOccurred += s,
                                                                                              s => _document.FileActionOccurred -= s);

                bufferChanged.Select(_ => false)
                             .Merge(fileAction.Where(a => a.EventArgs.FileActionType == FileActionTypes.ContentSavedToDisk)
                                              .Select(_ => true))
                             .Subscribe(Recompile)
                             .AddTo(_disposer);

                _compilerService.Listeners.AddListener(this);
                _compilerService.DocumentOpened(_document);
            } else {
                FilePath = "<unknown>";
            }

            var layer = _textView.GetAdornmentLayer("AdornmentLayer0");

            if (layer != null)
                _layer0 = new AdornmentLayer0(_textView, layer);

            textView.Closed += Closed;
        }

        private void Closed(object sender, EventArgs e)
        {
            if (_document != null)
                _compilerService.DocumentClosed(_document);

            if (_layer0 != null)
                _layer0.Dispose();

            _compilerService.Listeners.RemoveListener(this);
            _disposer.Dispose();
            _textView.Closed -= Closed;
        }

        private void Recompile(bool sendRuntimeUpdate)
        {
            _compilerService.Compile(FilePath, sendRuntimeUpdate);
        }

        public string FilePath { get; }

        public void Update(AmmyFile<Top> file)
        {
            var buffer = _textView.TextBuffer;

            if (file.Meta.Snapshot == null)
                return;

            try {
                if (buffer.Properties.ContainsProperty(typeof(ITagger<IOutliningRegionTag>))) {
                    var tagger = (OutliningTagger)buffer.Properties.GetProperty(typeof(ITagger<IOutliningRegionTag>));
                    tagger.Update(file);
                }

                if (buffer.Properties.ContainsProperty(typeof(ITagger<IErrorTag>))) {
                    var tagger = (ErrorTagger)buffer.Properties.GetProperty(typeof(ITagger<IErrorTag>));
                    tagger.Update(file);
                }

                if (buffer.Properties.ContainsProperty("AmmyClassifier")) {
                    var classifier = (Classifier)buffer.Properties.GetProperty("AmmyClassifier");
                    classifier.Update(file);
                }

                if (_layer0 != null)
                    _layer0.Update(file);
            } catch (Exception e) {
                Debug.WriteLine("Error updating services: " + e);
            }
        }
    }
}