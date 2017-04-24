using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace AmmySEA.Views
{
    public partial class QuestionBlockView
    {
        public QuestionBlockView()
        {
            InitializeComponent();
        }

        private void Navigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}
