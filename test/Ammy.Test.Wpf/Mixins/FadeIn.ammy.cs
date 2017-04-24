using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ammy.WpfTest.Mixins
{
    public partial class FadeIn
    {
        public FadeIn()
        {
            InitializeComponent();

            var tb = (TextBlock)Content;
            this.Assert(tb.Style.Triggers.Count == 3, "Fade In 3");
            this.Assert(tb.Style.Triggers.OfType<DataTrigger>().Count() == 1, "Fade In - DataTrigger");
            this.Assert(tb.Style.Triggers.OfType<EventTrigger>().Count() == 1, "Fade In - EventTrigger");
            this.Assert(tb.Style.Triggers.OfType<Trigger>().Count() == 1, "Fade In - Trigger");
        }
    }
}
