using System;
using System.Windows;
using System.Windows.Threading;

namespace AmmySidekick
{
    public class MainThread
    {
        public static void Run(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }
    }
}