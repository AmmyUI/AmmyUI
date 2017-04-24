using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AmmySidekick;

namespace DataGridSample
{
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            App app = new App();
            app.InitializeComponent();

            RuntimeUpdateHandler.Register(app, "/" + Ammy.GetAssemblyName(app) + ";component/App.g.xaml");

            app.Run();
        }
    }
}
