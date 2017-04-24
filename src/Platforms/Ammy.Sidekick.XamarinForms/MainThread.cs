using System;
using Xamarin.Forms;

namespace AmmySidekick
{
    public class MainThread
    {
        public static void Run(Action action)
        {
            Device.BeginInvokeOnMainThread(action);
        }
    }
}