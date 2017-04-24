using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ammy.WpfTest.Attributes
{
    public partial class Combine
    {
        public Combine()
        {
            InitializeComponent();

            this.Assert(Style.Triggers.Count == 2);
            this.Assert(((Trigger)Style.Triggers[0]).Property.Name == "Width");
            this.Assert((int)((double)((Trigger)Style.Triggers[0]).Value) == 50);

            this.Assert(((Trigger)Style.Triggers[1]).Property.Name == "Background");
            this.Assert(((Brush)((Trigger)Style.Triggers[1]).Value).Equals(Brushes.Green));

            this.Assert(Style.TargetType == typeof(UserControl));

            this.Assert(Style.Setters.Count == 2);
            this.Assert(((Setter)Style.Setters[0]).Property.Name == "Background");
            this.Assert(((Brush)((Setter)Style.Setters[0]).Value).Equals(Brushes.Red));

            this.Assert(((Setter)Style.Setters[1]).Property.Name == "Width");
            this.Assert((int)((double)((Setter)Style.Setters[1]).Value) == 100);

            this.Assert(TestGrid.RowDefinitions.Count == 3);
            this.Assert((int)TestGrid.RowDefinitions[0].Height.Value == 1);
            this.Assert((int)TestGrid.RowDefinitions[1].Height.Value == 2);
            this.Assert((int)TestGrid.RowDefinitions[2].Height.Value == 3);

            this.Assert(TestGrid.ColumnDefinitions.Count == 3);
            this.Assert((int)TestGrid.ColumnDefinitions[0].Width.Value == 50);
            this.Assert((int)TestGrid.ColumnDefinitions[1].Width.Value == 100);
            this.Assert((int)TestGrid.ColumnDefinitions[2].Width.Value == 150);

            this.Assert(TestGrid.Children.Count == 8);
            this.Assert(TestGrid.Children[0] is TextBlock);
            this.Assert(((TextBlock)TestGrid.Children[0]).Text == "1");
            this.Assert(TestGrid.Children[1] is Button);
            this.Assert(((string)((Button)TestGrid.Children[1]).Content) == "A");
            this.Assert(TestGrid.Children[2] is TextBlock);
            this.Assert(((TextBlock)TestGrid.Children[2]).Text == "2");
            this.Assert(TestGrid.Children[3] is Button);
            this.Assert(((string)((Button)TestGrid.Children[3]).Content) == "B");
            this.Assert(TestGrid.Children[4] is TextBlock);
            this.Assert(((TextBlock)TestGrid.Children[4]).Text == "3");
            this.Assert(TestGrid.Children[5] is Button);
            this.Assert(((string)((Button)TestGrid.Children[5]).Content) == "C");

            this.Assert(TestGrid.Children[6] is Grid);
            var grid0 = (Grid)TestGrid.Children[6];
            this.Assert(grid0.RowDefinitions.Count == 2);
            this.Assert((int)grid0.RowDefinitions[0].Height.Value == 1);
            this.Assert((int)grid0.RowDefinitions[1].Height.Value == 2);

            this.Assert(TestGrid.Children[7] is Grid);
            var grid1 = (Grid)TestGrid.Children[7];
            this.Assert(grid1.ColumnDefinitions.Count == 2);
            this.Assert((int)grid1.ColumnDefinitions[0].Width.Value == 3);
            this.Assert((int)grid1.ColumnDefinitions[1].Width.Value == 4);
        }
    }
}
