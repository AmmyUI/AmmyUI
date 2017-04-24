using System;
using System.Collections.Generic;

namespace Ammy.Platforms
{
    public class AvaloniaPlatform : IAmmyPlatform
    {
        public string Name => "Avalonia";
        public string OutputFileSuffix => "";
        public string XPrefix => "";
        public bool SupportsRuntimeUpdate => false;
        public PlatformTypeNames PlatformTypeNames => new AvaloniaTypeNames();
        public KeyValuePair<string, string>[] TopNodeAttributes => new[] {
            new KeyValuePair<string, string>("xmlns", "https://github.com/avaloniaui")
        };

        public string[] StaticPropertyImportList => new[] {
            "Avalonia.Media.Brushes",
            "Avalonia.Media.DashStyle",
        };


        public string[] DefaultNamespaces => new[] {
            "Avalonia",
            "Avalonia.Controls",
            "Avalonia.Controls.Presenters",
            "Avalonia.Controls.Primitives",
            "Avalonia.Controls.Shapes",
            "Avalonia.Styling"
        };

        public Type[] ProvideTypes()
        {
            return new Type[0];
        }
    }

    class AvaloniaTypeNames : PlatformTypeNames
    {
        public override string SetterBase => "Avalonia.Styling.Setter";
        public override string Style => "Avalonia.Styling.Style";
        public override string Thickness => "Avalonia.Thickness";
        public override string UIElement => "Avalonia.VisualTree.IVisual";
        public override string DependencyObject => "Avalonia.AvaloniaObject";
        public override string Binding => "Avalonia.Markup.Xaml.Data.Binding";
        public override string BindingBase => "Avalonia.Markup.Xaml.Data.Binding";
        public override string ResourceDictionary => "System.Windows.ResourceDictionary";
        public override string FrameworkElement => "Avalonia.Controls.Control";
        public override string DependencyProperty => "Avalonia.AvaloniaProperty";
        public override string RoutedEvent => "Avalonia.Interactivity.RoutedEvent";

    }
}