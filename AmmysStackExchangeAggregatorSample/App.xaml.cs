using Akavache;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace AmmySEA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            BlobCache.ApplicationName = "Ammy's Stack Exchange Aggregator";
            Current.Exit += ApplicationExit;
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            BlobCache.Shutdown().Wait();
        }
    }
}
