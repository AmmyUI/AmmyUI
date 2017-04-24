using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Ammy.VisualStudio.Service.Intellisense
{
    public class MixinSignature : ISignature
    {
        private IParameter _currentParameter;

        internal MixinSignature(ITextBuffer subjectBuffer, string content, string doc, ReadOnlyCollection<IParameter> parameters)
        {
            Buffer = subjectBuffer;
            Content = content;
            Documentation = doc;
            Parameters = parameters;
            Buffer.Changed += OnSubjectBufferChanged;
        }

        public ITextBuffer Buffer { get; }
        public ITrackingSpan ApplicableToSpan { get; set; }
        public string Content { get; }
        public string PrettyPrintedContent { get; }
        public string Documentation { get; }
        public ReadOnlyCollection<IParameter> Parameters { get; set; }
        public event EventHandler<CurrentParameterChangedEventArgs> CurrentParameterChanged;

        public IParameter CurrentParameter
        {
            get { return _currentParameter; }
            set {
                if (_currentParameter != value) {
                    var prevCurrentParameter = _currentParameter;
                    _currentParameter = value;
                    RaiseCurrentParameterChanged(prevCurrentParameter, _currentParameter);
                }
            }
        }

        internal void ComputeCurrentParameter()
        {
            if (Parameters.Count == 0) {
                CurrentParameter = null;
                return;
            }

            //the number of commas in the string is the index of the current parameter
            var sigText = ApplicableToSpan.GetText(Buffer.CurrentSnapshot);

            var currentIndex = 0;
            var commaCount = 0;
            while (currentIndex < sigText.Length) {
                var commaIndex = sigText.IndexOf(',', currentIndex);
                if (commaIndex == -1) {
                    break;
                }
                commaCount++;
                currentIndex = commaIndex + 1;
            }

            if (commaCount < Parameters.Count) {
                CurrentParameter = Parameters[commaCount];
            } else {
                //too many commas, so use the last parameter as the current one.
                CurrentParameter = Parameters[Parameters.Count - 1];
            }
        }

        public void OnSubjectBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            ComputeCurrentParameter();
        }

        private void RaiseCurrentParameterChanged(IParameter prevCurrentParameter, IParameter newCurrentParameter)
        {
            var args = new CurrentParameterChangedEventArgs(prevCurrentParameter, newCurrentParameter);
            CurrentParameterChanged?.Invoke(this, args);
        }
    }
}