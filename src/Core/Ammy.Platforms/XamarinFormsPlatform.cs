using System;
using System.Collections.Generic;

namespace Ammy.Platforms
{
    public class XamarinFormsPlatform : IAmmyPlatform
    {
        public string Name => "XamarinForms";
        public string OutputFileSuffix => ".g";
        public string XPrefix => "x:";
        public bool SupportsRuntimeUpdate => true;
        public PlatformTypeNames PlatformTypeNames => new XamarinTypeNames();
        public KeyValuePair<string, string>[] TopNodeAttributes => new[] {
            new KeyValuePair<string, string>("xmlns", "http://xamarin.com/schemas/2014/forms"), 
            new KeyValuePair<string, string>("xmlns:x", "http://schemas.microsoft.com/winfx/2009/xaml")
        };

        public string[] StaticPropertyImportList => new [] {
            "Xamarin.Forms.Color",
            "Xamarin.Forms.Easing",
            "Xamarin.Forms.LayoutOptions"
        };


        public string[] DefaultNamespaces => new[] {
            "Xamarin.Forms"
        };

        public Type[] ProvideTypes()
        {
            return new Type[0];
        }
    }

    public class XamarinTypeNames : PlatformTypeNames
    {
        public override string SetterBase         => "Xamarin.Forms.Setter";
        public override string Style              => "Xamarin.Forms.Style";
        public override string Thickness          => "Xamarin.Forms.Thickness";
        public override string UIElement          => "Xamarin.Forms.VisualElement";
        public override string DependencyObject   => "Xamarin.Forms.BindableObject";
        public override string DependencyProperty => "Xamarin.Forms.BindableProperty";
        public override string Binding            => "Xamarin.Forms.Binding";
        public override string BindingBase        => "Xamarin.Forms.BindingBase";
        public override string ResourceDictionary => "Xamarin.Forms.ResourceDictionary";
        public override string FrameworkElement   => "Xamarin.Forms.Element";
    }
}