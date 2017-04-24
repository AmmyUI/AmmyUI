using System.Windows.Input;

namespace AmmyTest.Common
{
    public class Helpers
    {
        public static void OnlyNumeric(object sender, TextCompositionEventArgs e)
        {
            double result;
            e.Handled = !double.TryParse(e.Text, out result);
        }
    }
}
