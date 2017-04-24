using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ammy.Test.Workbench
{
    public class DoubleValidation : ValidationRule
    {
        public bool CanBeNull { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public string ValueName { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return null;
        }
    }
}


namespace Ammy.Test.Workbench2
{
    public class TextBlockExtensions
    {
        public static readonly DependencyProperty RemoveEmptyRunsProperty =
        DependencyProperty.RegisterAttached("RemoveEmptyRuns", typeof(bool), typeof(TextBlock), new PropertyMetadata(false));

        public static bool GetRemoveEmptyRuns(DependencyObject obj) { return true; }
        public static void SetRemoveEmptyRuns(DependencyObject obj, bool value) { }
    }

    public enum Gender { Male, Female };

    public class DataContextClass
    {
        public Gender Gender { get; set; }
    }

    public class DoubleValidation2 : ValidationRule
    {
        public bool CanBeNull { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public string ValueName { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return null;
        }
    }
}

