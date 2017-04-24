using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AmmySidekick
{
    public static partial class Ammy
    {
        private static readonly Queue<Tuple<Element, string>> InitializationQueue = new Queue<Tuple<Element, string>>();
        private static bool _isInitializing;

        private static void LoadComponent(Element fe, string componentId, bool selfCall = false)
        {
            var split = componentId.Split(',');

            foreach (var value in split) {
                try {
                    var buffer = RuntimeUpdateHandler.FindBuffer(value);
                    if (buffer != null) {
                        RuntimeUpdateHandler.InitializeComponent((View)fe, buffer);
                    } else {
                        fe.LoadFromXaml(fe.GetType());
                    }
                } catch (Exception e) {
                    Debug.WriteLine("Failed to initialize with: " + value);
                    Debug.WriteLine(e.Message);
                }
            }
        }

        private static void ProcessQueue()
        {
            while (InitializationQueue.Count > 0) {
                var tuple = InitializationQueue.Dequeue();
                LoadComponent(tuple.Item1, tuple.Item2, true);
            }
        }
        /*
        private static void AfterInitialize(Element fe, Action<Element> action)
        {
            EventHandler onInitialized = null;
            onInitialized = (sender, args) => {
                fe.Initialized -= onInitialized;
                action(fe);
            };
            fe.Initialized += onInitialized;
        }

        private static void AfterLoad(Element fe, Action<Element> action)
        {
            RoutedEventHandler onLoaded = null;
            onLoaded = (sender, args) => {
                fe.Loaded -= onLoaded;
                action(fe);
            };
            fe.Loaded += onLoaded;
        }*/
    }
}