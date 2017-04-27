using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Ammy.WpfTest.Attributes
{
    public partial class NodeKey
    {
        public NodeKey()
        {
            InitializeComponent();

            this.Assert(Resources.Count == 3, "Resource count should be 3, actual value " + Resources.Count);

            var a = (SolidColorBrush)Resources["a"];
            var b = (SolidColorBrush)Resources["b"];
            var c = (SolidColorBrush)Resources["c"];

            this.Assert(a.ToString() == "#FFAAAAAA", "AAAAAA");
            this.Assert(b.ToString() == "#FFBBBBBB", "BBBBBB");
            this.Assert(c.ToString() == "#FFCCCCCC", "CCCCCC");
        }
    }
}
