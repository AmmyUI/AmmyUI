using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
#else
using Xamarin.Forms;
#endif

namespace AmmySidekick
{
    public class ExpressionConverter : IValueConverter
    {
        public static ExpressionConverter Instance = new ExpressionConverter();
        private readonly Dictionary<string, Delegate> _converterCache = new Dictionary<string, Delegate>();

#if WINDOWS_UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
#endif
        {
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
        }

#if WINDOWS_UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
#endif
        {
            throw new NotImplementedException();
        }
    }
}