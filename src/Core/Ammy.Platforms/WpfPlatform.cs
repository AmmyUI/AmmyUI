using System;
using System.Collections.Generic;

namespace Ammy.Platforms
{
    public class WpfPlatform : IAmmyPlatform
    {
        public string Name => "WPF";
        public string OutputFileSuffix => ".g";
        public string XPrefix => "x:";
        public bool SupportsRuntimeUpdate { get; set; }
        public PlatformTypeNames PlatformTypeNames => new PlatformTypeNames();
        public KeyValuePair<string, string>[] TopNodeAttributes => new [] {
            new KeyValuePair<string, string>("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"),
            new KeyValuePair<string, string>("xmlns:x", "http://schemas.microsoft.com/winfx/2006/xaml")
        };

        public string[] StaticPropertyImportList => new[] {
            "System.Windows.FontStyles",
            "System.Windows.FontWeights",
            "System.Windows.FontStretches",
            "System.Windows.TextDecorations",
            "System.Windows.Media.Brushes",
            "System.Windows.Media.DashStyles",
        };
        
        public string[] DefaultNamespaces => new[] {
          "System.Windows",
          "System.Windows.Automation",
          "System.Windows.Controls",
          "System.Windows.Controls.Primitives",
          "System.Windows.Controls.Shapes",
          "System.Windows.Data",
          "System.Windows.Documents",
          "System.Windows.Forms.Integration",
          "System.Windows.Ink",
          "System.Windows.Input",
          "System.Windows.Media",
          "System.Windows.Media.Animation",
          "System.Windows.Media.Effects",
          "System.Windows.Media.Imaging",
          "System.Windows.Media.Media3D",
          "System.Windows.Media.TextFormatting",
          "System.Windows.Navigation",
          "System.Windows.Shapes",
          "System.Windows.Shell"
        };

        public Type[] ProvideTypes()
        {
            return new Type[0];
        }

        public WpfPlatform()
        {
            SupportsRuntimeUpdate = true;
        }
    }
}