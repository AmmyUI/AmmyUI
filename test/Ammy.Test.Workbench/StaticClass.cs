using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.TreeList;

namespace Ammy.Test.Workbench
{
    public class EventArgsToEventPatternContenter : EventArgsConverterBase<EventArgs>
    {
        protected override object Convert(object sender, EventArgs args) => null;
    }
    
    public class CheckBoxGridColumnHelper
    {
        public static readonly ICommand CheckEditPreviewMouseDown = null;        
    }
}