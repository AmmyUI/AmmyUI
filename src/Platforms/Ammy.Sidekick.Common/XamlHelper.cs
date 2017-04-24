using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace AmmySidekick
{
    public class XamlHelper
    {
        public static void InitializeComponent(FrameworkElement frameworkElement, string baseUri)
        {
            var resourceLocator = new Uri(baseUri, UriKind.Relative);
            var applicationType = typeof(Application);
            var getContentPartMethod = applicationType.GetMethod("GetResourceOrContentPart", BindingFlags.NonPublic | BindingFlags.Static);
            var packagePart = (PackagePart)getContentPartMethod.Invoke(null, new object[] { resourceLocator });
            Func<Stream> streamFactory = () => packagePart.GetStream();

            LoadBaml(frameworkElement, streamFactory, resourceLocator);
        }

        public static void InitializeComponent(object frameworkElement, byte[] bamlBuffer, Uri resourceLocator)
        {
            Func<Stream> streamFactory = () => {
                var ms = new MemoryStream(bamlBuffer);
                var bamlStreamType = Type.GetType("MS.Internal.AppModel.BamlStream, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                var streamCtor = bamlStreamType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).First();

                return (Stream)streamCtor.Invoke(new object[] { ms, typeof(XamlHelper).Assembly });
            };

            LoadBaml(frameworkElement, streamFactory, resourceLocator);
        }

        private static void LoadBaml(object frameworkElement, Func<Stream> streamFactory, Uri resourceLocator)
        {
            if (_isInitializing) {
                InitializationQueue.Enqueue(() => LoadBamlImpl(frameworkElement, streamFactory, resourceLocator));
            } else {
                LoadBamlImpl(frameworkElement, streamFactory, resourceLocator);
            }
        }

        private static void LoadBamlImpl(object rootObject, Func<Stream> streamFactory, Uri resourceLocator)
        {
            _isInitializing = true;

            try {
                using (var stream = streamFactory()) {
                    var uri = new Uri(new Uri("pack://application:,,,", UriKind.Absolute), resourceLocator);
                    var parserContext = new ParserContext {
                        BaseUri = uri
                    };
                    var xamlReaderType = typeof (XamlReader);
                    var loadBamlMethod = xamlReaderType.GetMethod("LoadBaml", BindingFlags.NonPublic | BindingFlags.Static);

                    loadBamlMethod.Invoke(null, new object[] {stream, parserContext, rootObject, true});
                }
            } catch (Exception e) {
                var frameworkElement = rootObject as FrameworkElement;
                RuntimeUpdateHandler.ClearChildren(frameworkElement);

                if (frameworkElement != null)
                    frameworkElement.SetValue(Control.BackgroundProperty, Brushes.Red);

                try {
                    if (frameworkElement != null)
                        frameworkElement.EndInit();
                } catch { }
                
                if (e is TargetInvocationException && e.InnerException != null) {
                    Debug.WriteLine("Runtime update failed: " + e.InnerException);
                    MessageBox.Show(e.InnerException.ToString());
                } else {
                    Debug.WriteLine("Runtime update failed: " + e);
                    MessageBox.Show(e.ToString());
                }
            }

            if (InitializationQueue.Count > 0) {
                var nextInitializer = InitializationQueue.Dequeue();
                nextInitializer();
            } else {
                _isInitializing = false;
            }
        }

        private static bool _isInitializing;
        private static readonly Queue<Action> InitializationQueue = new Queue<Action>();
    }
}