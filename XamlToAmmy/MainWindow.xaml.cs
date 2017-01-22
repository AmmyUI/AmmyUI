using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace XamlToAmmy
{
    public partial class MainWindow : Window
    {
        readonly XamlToAmmyConverter _converter = new XamlToAmmyConverter();

        public MainWindow()
        {
            InitializeComponent();

            XamlInput.TextChanged += XamlChanged;
        }

        private void XamlChanged(object sender, TextChangedEventArgs e)
        {
            try {
                AmmyOutput.Text = _converter.Convert(XamlInput.Text);
            }
            catch (Exception exception) {
                AmmyOutput.Text = exception.ToString();
            }
        }
    }

    public class XamlToAmmyConverter
    {
        public string Convert(string xaml)
        {
            xaml = xaml.Replace("x:", "x__");
            var doc = XDocument.Parse(xaml);
            var root = doc.Root;
            var sb = new StringBuilder();

            ConvertElement(root, sb);

            return sb.ToString();
        }

        private void ConvertElement(XElement el, StringBuilder sb, string indent = "", bool isValue = false)
        {
            var name = el.Attribute("x__Name");
            var key = el.Attribute("x__Key");

            if (!isValue)
                sb.Append(indent);
            sb.Append(el.Name);

            if (name != null)
                sb.Append(" \"" + name.Value + "\"");

            if (key != null)
                sb.Append(" Key=\"" + key.Value + "\"");

            sb.Append(" {");
            sb.Append(Environment.NewLine);

            if (!string.IsNullOrEmpty(el.Value))
                sb.Append(indent + "  \"" + el.Value + "\"" + Environment.NewLine);

            var attrs = el.Attributes().Where(a => a.Name != "x__Name" && a.Name != "x__Key").ToList();
            for (int i = 0; i < attrs.Count; i++) {
                var attr = attrs[i];
                sb.Append(indent + "  ");
                sb.Append(attr.Name);
                sb.Append(": ");
                sb.Append("\"" + attr.Value + "\"");
                sb.Append(Environment.NewLine);
            }

            foreach (var child in el.Elements()) {
                var attrChildPrefix = el.Name + ".";
                var localName = child.Name.LocalName;

                if (localName.StartsWith(attrChildPrefix)) {
                    var attrName = localName.Substring(attrChildPrefix.Length);
                    ConvertElementAttribute(sb, indent, attrName, child);
                } else if (localName.Contains(".")) {
                    ConvertElementAttribute(sb, indent, localName, child);
                } else {
                    ConvertElement(child, sb, indent + "  ");
                }   
            }
            
            sb.Append(indent);
            sb.Append("}");
            sb.Append(Environment.NewLine);
        }

        private void ConvertElementAttribute(StringBuilder sb, string indent, string attrName, XElement child)
        {
            sb.Append(indent + "  ");
            sb.Append(attrName);
            sb.Append(": ");

            var values = child.Elements().ToList();
            if (values.Count > 1) {
                sb.Append("[");
                sb.Append(Environment.NewLine);
                foreach (var value in values)
                    ConvertElement(value, sb, indent + "    ");
                sb.Append(indent + "  ");
                sb.Append("]");
            }
            else if (values.Count == 1) {
                ConvertElement(values[0], sb, indent + "  ", true);
            }
            else {
                sb.Append(child.Value);
            }

            sb.Append(Environment.NewLine);
        }
    }
}
