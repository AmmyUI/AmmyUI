using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Xml.Linq;

namespace AmmySidekick
{
    public class ExpressionConverter : IValueConverter
    {
        public static ExpressionConverter Instance = new ExpressionConverter();
        private readonly Dictionary<string, Delegate> _converterCache = new Dictionary<string, Delegate>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try {
                var builder = new ExpressionBuilder(this);
                var doc = XDocument.Parse(parameter.ToString());
                var id = doc.Root.Attribute("id").Value;

                Delegate converter;

                if (_converterCache.TryGetValue(id, out converter))
                    return converter.DynamicInvoke(value);
                
                var lambda = builder.Build(doc);
                var compiled = lambda.Compile();

                _converterCache[id] = compiled;

                var result = compiled.DynamicInvoke(value);
                return result;
            } catch {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}