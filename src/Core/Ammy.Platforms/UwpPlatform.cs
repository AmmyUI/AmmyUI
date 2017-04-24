using System;
using System.Collections.Generic;

namespace Ammy.Platforms
{
    public class UwpPlatform : IAmmyPlatform
    {
        public string Name => "UWP";
        public string OutputFileSuffix => ".g";
        public string XPrefix => "x:";
        public bool SupportsRuntimeUpdate => false;
        public PlatformTypeNames PlatformTypeNames => new UwpPlatformTypeNames();
        public KeyValuePair<string, string>[] TopNodeAttributes => new[] {
            new KeyValuePair<string, string>("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"),
            new KeyValuePair<string, string>("xmlns:x", "http://schemas.microsoft.com/winfx/2006/xaml")
        };

        public string[] StaticPropertyImportList => new[] {
            "Windows.UI.Colors",
            "Windows.UI.Text"
        };

        public string[] DefaultNamespaces => new[] {
            "Windows.UI.Xaml",
            "Windows.UI.Xaml.Documents",
            "Windows.UI.Xaml.Data",
            "Windows.UI.Xaml.Controls",
            "Windows.UI.Xaml.Controls.Maps",
            "Windows.UI.Xaml.Primitives",
            "Windows.UI.Xaml.Shapes",
            "Windows.UI.Xaml.Media",
            "Windows.UI.Xaml.Media.Animation",
            "Windows.UI.Xaml.Media.Imaging",
            "Windows.UI.Xaml.Media.Media3D",
        };

        public Type[] ProvideTypes()
        {
            return new Type[0];
        }
    }

    public class UwpPlatformTypeNames : PlatformTypeNames
    {
        public override string SetterBase => "Windows.UI.Xaml.SetterBase";
        public override string Style => "Windows.UI.Xaml.Style";
        public override string Thickness => "Windows.UI.Xaml.Thickness";
        public override string UIElement => "Windows.UI.Xaml.UIElement";
        public override string ICommand => "Windows.UI.Xaml.Input.ICommand";
        public override string DependencyObject => "Windows.UI.Xaml.DependencyObject";
        public override string Binding => "Windows.UI.Xaml.Data.Binding";
        public override string BindingBase => "Windows.UI.Xaml.Data.BindingBase";
        public override string ResourceDictionary => "Windows.UI.Xaml.ResourceDictionary";
        public override string FrameworkElement => "Windows.UI.Xaml.FrameworkElement";
        public override string DependencyProperty => "Windows.UI.Xaml.DependencyProperty";
        public override string RoutedEvent => "Windows.UI.Xaml.RoutedEvent";
    }
}