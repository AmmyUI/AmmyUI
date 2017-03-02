using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using XamlToAmmy.ViewModels;

namespace XamlToAmmy
{
    public partial class MainWindow : IOpenFileDialog
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel(this);
            InitializeComponent();
        }

        public IObservable<string> BrowseFile()
        {
            return Observable.Create<string>(obs => {
                var dialog = new OpenFileDialog {
                    Multiselect = false,
                    Filter = "Project files|*.csproj"
                };

                if (dialog.ShowDialog() == true) {
                    var fileName = dialog.FileName;
                    obs.OnNext(fileName);
                }

                obs.OnCompleted();

                return Disposable.Empty;
            });
        }
    }
}
