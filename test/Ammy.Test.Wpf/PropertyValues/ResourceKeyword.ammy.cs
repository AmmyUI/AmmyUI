using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ammy.WpfTest.PropertyValues
{
    public partial class ResourceKeyword
    {
        public ResourceKeyword()
        {
            InitializeComponent();

            var testBrush = Resources["brush0"];

            this.Assert(TestTextBlock.Background == testBrush);
            this.Assert(TestTextBlock.Foreground == testBrush);
        }
    }
}
