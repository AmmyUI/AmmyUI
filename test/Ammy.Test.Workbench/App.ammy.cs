using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AmmySidekick;

namespace Ammy.Test.Workbench
{
  public partial class App : Application
  {
    [STAThread]
    public static void Main()
    {
      App app = new App();
      app.InitializeComponent();

      RuntimeUpdateHandler.Register(app, "/" + AmmySidekick.Ammy.GetAssemblyName(app) + ";component/App.g.xaml");

      app.Run();
    }
  }
}
