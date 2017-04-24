using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ammy.Test.Workbench2;

namespace Ammy.Test.Workbench
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new Context() ;
        }
    }


    public class Context : INotifyPropertyChanged
    {
        public string Xaml { get; set; }
        
        public Context()
        {
            PropertyChanged += Context_PropertyChanged;
        }

        private void Context_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine("Property changed: " + e.PropertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
