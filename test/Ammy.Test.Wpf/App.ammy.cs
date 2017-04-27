using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ammy.WpfTest.Attributes;
using Ammy.WpfTest.Mixins;
using AmmySidekick;

namespace Ammy.WpfTest
{
    public partial class App : Application
    {
        private bool _isSuccess = true;

        public App()
        {
            Test(() => new Combine());
            Test(() => new FadeIn());
            Test(() => new NodeKey());
            
            Current.Shutdown(_isSuccess ? 0 : -1);
        }

        void Test(Func<object> action)
        {
            try {
                var obj = action();
                if (obj != null)
                    Console.WriteLine(obj.GetType().Name + " passed");
            } catch (Exception e) {
                Console.Error.WriteLine("Fail: " + e.Message);
                _isSuccess = false;
            }
        }

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
