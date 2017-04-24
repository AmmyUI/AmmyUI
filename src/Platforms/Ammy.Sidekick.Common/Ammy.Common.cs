using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace AmmySidekick
{
    public static partial class Ammy
    {
        private static readonly Queue<Tuple<FrameworkElement, string>> InitializationQueue = new Queue<Tuple<FrameworkElement, string>>();
        private static bool _isInitializing;
        private static Listener _listenerInstance;

        private static void LoadComponent(FrameworkElement fe, string componentId, bool selfCall = false)
        {
            // Don't load component in design mode
            var isDesignMode = (bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue;
            if (Application.Current == null || isDesignMode)
                return;

                //if (_isInitializing && !selfCall) {
                //    InitializationQueue.Enqueue(Tuple.Create(fe, componentId));
                //    return;
                //}

                //_isInitializing = true;

            var split = componentId.Split(',');
            var resourceAssembly = Assembly.GetEntryAssembly();

            if (resourceAssembly == null)
                return;

            foreach (var value in split) {
                try {
                    var buffer = RuntimeUpdateHandler.FindBuffer(value);
                    if (buffer != null) {
                        XamlHelper.InitializeComponent(fe, buffer, new Uri(value, UriKind.Relative));
                    } else {
                        XamlHelper.InitializeComponent(fe, value);
                    }
                } catch (Exception e) {
                    Debug.WriteLine("Failed to initialize with: " + value);
                    Debug.WriteLine(e.Message);
                }
            }

            //ProcessQueue();

            //_isInitializing = false;
        }

        private static void ProcessQueue()
        {
            while (InitializationQueue.Count > 0) {
                var tuple = InitializationQueue.Dequeue();
                LoadComponent(tuple.Item1, tuple.Item2, true);
            }
        }

        private static void AfterInitialize(FrameworkElement fe, Action<FrameworkElement> action)
        {
            EventHandler onInitialized = null;
            onInitialized = (sender, args) => {
                fe.Initialized -= onInitialized;
                action(fe);
            };
            fe.Initialized += onInitialized;
        }

        private static void AfterLoad(FrameworkElement fe, Action<FrameworkElement> action)
        {
            RoutedEventHandler onLoaded = null;
            onLoaded = (sender, args) => {
                fe.Loaded -= onLoaded;
                action(fe);
            };
            fe.Loaded += onLoaded;
        }

        private static void StartListenerIfNeeded()
        {
            try {
                var isDesignMode = (bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof (DependencyObject)).DefaultValue;
                if (Application.Current != null && !isDesignMode)
                    _listenerInstance = Listener.Instance;
            } catch (Exception e) {
                Debug.WriteLine(e);
                throw;
            }
        }
    }
}