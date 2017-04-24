using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using PropertyChanged;
using ReactiveUI;

namespace XamlToAmmy.ViewModels
{
    [ImplementPropertyChanged]
    public class ConvertFileViewModel
    {
        public string Xaml { get; set; }
        public string Ammy { get; set; }
        public string Warnings { get; private set; }
        public string[] PageFilenames { get; set; }

        private readonly FileConverter _fileConverter;
        private readonly SettingsViewModel _settings;

        public ConvertFileViewModel(FileConverter fileConverter, SettingsViewModel settings)
        {
            _fileConverter = fileConverter;
            _settings = settings;

            PageFilenames = new string[0];

            var settingsChanged = GetSettingsChanged(settings);

            this.WhenAnyValue(vm => vm.Xaml)
                .Merge(settingsChanged.Select(_ => Xaml))
                .Where(xaml => xaml != null)
                .Throttle(TimeSpan.FromMilliseconds(200), RxApp.MainThreadScheduler)
                .Select(ConvertXamlToAmmy)
                .BindTo(this, vm => vm.Ammy);
        }

        private object ConvertXamlToAmmy(string xaml)
        {
            try {
                var ammy = _fileConverter.Convert(xaml, PageFilenames, _settings.CollapsedNodeMaxSize, _settings.IndentSize, _settings.OpeningBraceOnNewLine);

                Warnings = string.Join(Environment.NewLine, _fileConverter.Warnings);

                return ammy;
            } catch (Exception e) {
                return e.ToString();
            }
        }

        private static IObservable<EventPattern<PropertyChangedEventArgs>> GetSettingsChanged(SettingsViewModel settings)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var settingsInpc = (INotifyPropertyChanged)settings;
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>
                    (h => settingsInpc.PropertyChanged += h, h => settingsInpc.PropertyChanged -= h);
        }
    }
}