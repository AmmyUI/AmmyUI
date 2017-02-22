using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace XamlToAmmy
{
    [ImplementPropertyChanged]
    class MainWindowViewModel
    {
        public string Xaml { get; set; }
        public string Ammy { get; set; }

        public MainWindowViewModel()
        {
        }
    }
}
