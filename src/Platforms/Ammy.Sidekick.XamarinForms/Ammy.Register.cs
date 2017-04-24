using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace AmmySidekick
{
    public static partial class Ammy
    {
        public static readonly BindableProperty RegisterProperty = BindableProperty.CreateAttached("Register", typeof(string), typeof(Ammy), "", BindingMode.OneWay, null, RegisterPropertyChanged);

        private static Listener _listenerInstance;

        private static void RegisterPropertyChanged(BindableObject dependencyObject, object oldValue, object newValue)
        {
            var fe = dependencyObject as Element;
            if (fe == null)
                return;

            StartListenerIfNeeded();

            var componentId = (string)newValue;

            if (!componentId.Equals(RuntimeUpdateHandler.CurrentlyUpdatedTargetId, StringComparison.OrdinalIgnoreCase)) {
                Device.BeginInvokeOnMainThread(() => {
                    var initialPropertyList = RuntimeUpdateHandler.GetInitialPropertyList(componentId) ?? "";
                    RuntimeUpdateHandler.ClearElement(fe, componentId, initialPropertyList);
                    LoadComponent(fe, componentId);
                });
            }

            Device.BeginInvokeOnMainThread(() => {
                RuntimeUpdateHandler.Register(fe, componentId);
            });
        }

        public static void SetRegister(BindableObject element, string value)
        {
            element.SetValue(RegisterProperty, value);
        }

        public static string GetRegister(BindableObject element)
        {
            return (string)element.GetValue(RegisterProperty);
        }

        private static void StartListenerIfNeeded()
        {
            try {
                if (Application.Current != null)
                    _listenerInstance = Listener.Instance;
            } catch (Exception e) {
                Debug.WriteLine(e);
                throw;
            }
        }
    }
}