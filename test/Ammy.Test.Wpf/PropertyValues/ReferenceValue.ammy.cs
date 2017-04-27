using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ammy.WpfTest.PropertyValues
{
    public partial class ReferenceValue
    {
        public ReferenceValue()
        {
            InitializeComponent();

            this.Assert(TextBlockWithConst.Text == Utils.ConstValue);
        }
    }
}
