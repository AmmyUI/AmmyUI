using System;
using System.Windows;

namespace AmmySidekick
{
    public static partial class Ammy
    {
        public static readonly DependencyProperty RegisterProperty = DependencyProperty.RegisterAttached(
       "Register", typeof(string), typeof(Ammy), new PropertyMetadata("", RegisterPropertyChanged));

        private static void RegisterPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            StartListenerIfNeeded();

            var fe = dependencyObject as FrameworkElement;
            if (fe == null)
                return;

            var componentId = (string)eventArgs.NewValue;
            var alreadyRegistered = RuntimeUpdateHandler.IsRegistered(componentId);

            if (alreadyRegistered && !componentId.Equals(RuntimeUpdateHandler.CurrentlyUpdatedTargetId, StringComparison.InvariantCultureIgnoreCase)) {
                AfterInitialize(fe, f => {
                    var initialPropertyList = RuntimeUpdateHandler.GetInitialPropertyList(componentId) ?? "";
                    RuntimeUpdateHandler.ClearElement(fe, componentId, initialPropertyList);
                    LoadComponent(f, componentId);
                });
            }

            AfterLoad(fe, f => {
                RuntimeUpdateHandler.Register(f, componentId);
            });
        }

        public static void SetRegister(DependencyObject element, string value)
        {
            element.SetValue(RegisterProperty, value);
        }

        public static string GetRegister(DependencyObject element)
        {
            return (string)element.GetValue(RegisterProperty);
        }
    }
}