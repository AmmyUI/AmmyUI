using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace XamlToAmmy
{
    public class FileConverter
    {
        private readonly List<string> _warnings = new List<string>();
        private readonly List<string> _textUsings = new List<string>();
        private readonly Dictionary<string, string> _namespaces = new Dictionary<string, string>();
        private readonly Dictionary<string, string[]> _knownNamespaceMappings = new Dictionary<string, string[]>();
        private string _xNamespaceName;
        private XAttribute _xclass;
        private string _mcNamespace;
        private string _dNamespace;
        private XNamespace _defaultNamespaceName;
        private int _collapsedNodeMaxSize;
        private string _indentIncr;
        private bool _openingBraceOnNewLine;
        private int _firstElementBodyIndex;
        private string[] _allPages;

        public IReadOnlyList<string> Warnings => _warnings;

        public FileConverter()
        {
            _knownNamespaceMappings = new Dictionary<string, string[]> {
                { "http://metro.mahapps.com/winfx/xaml/controls", new[] { "MahApps.Metro.Controls" } },
                { "http://metro.mahapps.com/winfx/xaml/shared", new[] { "MahApps.Metro.Converters", "MahApps.Metro.Behaviours"} }
            };
        }

        public string Convert(string xaml, string[] allPages, int collapsedNodeMaxSize, int indentSize, bool openingBraceOnNewLine)
        {
            var doc = XDocument.Parse(xaml);
            var root = doc.Root;

            if (root == null)
                return "";

            var sb = new StringBuilder();

            _allPages = allPages;
            _collapsedNodeMaxSize = collapsedNodeMaxSize;
            _indentIncr = new string(' ', indentSize);
            _openingBraceOnNewLine = openingBraceOnNewLine;

            _warnings.Clear();
            _textUsings.Clear();

            _defaultNamespaceName = root.GetDefaultNamespace();
            _namespaces[""] = _defaultNamespaceName.NamespaceName;

            var nsDecls = root.Attributes()
                              .Where(a => a.IsNamespaceDeclaration);

            foreach (var nsDeclaration in nsDecls) {
                _namespaces[nsDeclaration.Name.LocalName] = nsDeclaration.Value;

                if (IsNonStandardNamespace(nsDeclaration.Name.LocalName))
                    TryAppendUsing(nsDeclaration, sb);
            }

            if (sb.Length > 0)
                sb.AppendLine();

            _xNamespaceName = _namespaces.ContainsKey("x") ? _namespaces["x"] : "";
            _mcNamespace = _namespaces.ContainsKey("mc") ? _namespaces["mc"] : null;
            _dNamespace = _namespaces.ContainsKey("d") ? _namespaces["d"] : null;
            _xclass = FindAttribute(root, _xNamespaceName, "Class");
            
            if (_xclass != null)
                ConvertElement(root, sb, "", false, _xclass.Value, true);
            else
                ConvertElement(root, sb, "", false, null, true);

            foreach (var textUsing in _textUsings.Distinct())
                sb.Insert(_firstElementBodyIndex, Environment.NewLine + _indentIncr + textUsing);

            return sb.ToString();
        }

        private void TryAppendUsing(XAttribute nsDeclaration, StringBuilder sb)
        {
            var match = Regex.Match(nsDeclaration.Value, @"clr-namespace:(@?[a-z_A-Z]\w+(?:\.@?[a-z_A-Z]\w+)*)");

            if (match.Success && match.Groups.Count == 2)
                sb.AppendLine("using " + match.Groups[1].Value + ";");
            else {
                string[] namespaces;

                if (_knownNamespaceMappings.TryGetValue(nsDeclaration.Value, out namespaces)) {
                    foreach (var ns in namespaces)
                        sb.AppendLine("using " + ns + ";");
                } else {
                    _warnings.Add("Non-standard namespace definition found: " + nsDeclaration.Name.LocalName + "=\"" + nsDeclaration.Value + "\"" + Environment.NewLine +
                                  "You will have to import CLR namespace manually.");
                }
            }
        }

        private void ConvertElement(XElement el, StringBuilder sb, string indent = "", bool isValue = false, string overridenName = null, bool isFirstElement = false)
        {
            var name = FindAttribute(el, _xNamespaceName, "Name");
            var key = FindAttribute(el, _xNamespaceName, "Key");
            
            if (!isValue && !isFirstElement)
                sb.Append(Environment.NewLine + indent);
            
            AppendHeader(el, sb, overridenName, name, key);
            AppendBody(el, sb, indent, name, key, isFirstElement);
        }

        private static void AppendHeader(XElement el, StringBuilder sb, string overridenName, XAttribute name, XAttribute key)
        {
            sb.Append(el.Name.LocalName);

            if (el.Name.LocalName == "StaticResource")
                sb.Append("Extension");

            if (overridenName != null)
                sb.Append(" \"" + overridenName + "\"");
            else if (name != null)
                sb.Append(" \"" + name.Value + "\"");

            if (key != null)
                sb.Append(" Key=\"" + key.Value + "\"");
        }

        private void AppendBody(XElement el, StringBuilder sb, string indent, XAttribute name, XAttribute key, bool isFirstElement = false)
        {
            var nodeCount = el.Nodes().Count();
            var attributeCount = el.Attributes().Count();
            var isSimpleNode = attributeCount <= _collapsedNodeMaxSize && nodeCount == 0;
            
            if (!isSimpleNode && _openingBraceOnNewLine) {
                sb.Append(Environment.NewLine + indent + "{");
            } else {
                sb.Append(" {");
            }

            if (isFirstElement)
                _firstElementBodyIndex = sb.Length;

            AppendAttributes(el, sb, indent, name, key, isSimpleNode);

            //if (el.Nodes().Any())
            //    sb.AppendLine();

            AppendNodes(el, sb, indent, isSimpleNode);

            if (!isSimpleNode)
                sb.Append(Environment.NewLine + indent + "}");
            else
                sb.Append(" }");
        }

        private void AppendAttributes(XElement el, StringBuilder sb, string indent, XAttribute name, XAttribute key, bool isSimpleNode)
        {
            var attrs = el.Attributes()
                .Where(a => !a.IsNamespaceDeclaration)
                .Where(a => a != name &&
                            a != key &&
                            a != _xclass)
                .Where(a => a.Name.NamespaceName != _dNamespace)
                .Where(a => a.Name.NamespaceName != _mcNamespace)
                .ToList();

            var propertyList = new List<string>();

            for (int i = 0; i < attrs.Count; i++) {
                var attr = attrs[i];
                var attrPrefix = isSimpleNode ? " " : indent + _indentIncr;
                var property = attrPrefix + ResolveAttributeName(attr) + ": " + ResolveValue(attr);

                propertyList.Add(property);
            }

            if (!isSimpleNode) {
                if (propertyList.Count > 0)
                    sb.AppendLine();

                sb.Append(string.Join(Environment.NewLine, propertyList));
            } else {
                sb.Append(string.Join(", ", propertyList));
            }
        }

        private string ResolveValue(XAttribute attr)
        {
            var value = attr.Value;

            var xTypeMatch = Regex.Match(value, @"^{x:Type (\w+:)?(\w+)}$");
            if (xTypeMatch.Success && xTypeMatch.Groups.Count >= 1)
                return xTypeMatch.Groups[xTypeMatch.Groups.Count - 1].Value;

            var hasNsPrefixMatch = Regex.Match(value, @"(^(\w+):\w+)|(\s(\w+):\w+)");
            if (hasNsPrefixMatch.Success && hasNsPrefixMatch.Groups.Count >= 5) {
                var nsName = "";

                if (hasNsPrefixMatch.Groups[2].Success)
                    nsName = hasNsPrefixMatch.Groups[2].Value;
                else if (hasNsPrefixMatch.Groups[4].Success)
                    nsName = hasNsPrefixMatch.Groups[4].Value;

                string ns;
                if (nsName != "x" && !string.IsNullOrEmpty(nsName) && _namespaces.TryGetValue(nsName, out ns))
                    _textUsings.Add($"\"xmlns:{nsName}\": \"{ns}\"");
            }

            value = ResolveLocalPage(value);

            return "\"" + value + "\"";
        }

        private string ResolveLocalPage(string value)
        {
            if (Array.IndexOf(_allPages, value) != -1 || Array.IndexOf(_allPages, value.TrimStart('/')) != -1)
                value = Path.ChangeExtension(value, ".g.xaml");
            return value;
        }

        private string ResolveAttributeName(XAttribute attr)
        {
            if (attr.Name.NamespaceName == _xNamespaceName)
                return "\"x:" + attr.Name.LocalName + "\"";

            return attr.Name.LocalName;
        }

        private void AppendNodes(XElement el, StringBuilder sb, string indent, bool isSimpleNode)
        {
            foreach (var node in el.Nodes()) {
                if (node.NodeType == XmlNodeType.Text) {
                    var xtext = (XText)node;                    
                    var value = xtext.Value.Trim().Replace("\"", "\\\"");

                    sb.Append(Environment.NewLine + indent + _indentIncr + "\"" + value + "\"");
                } else if (node.NodeType == XmlNodeType.Element) {
                    var child = (XElement)node;
                    var attrChildPrefix = el.Name + ".";
                    var childName = child.Name.ToString();
                    var localName = child.Name.LocalName;
                    var parentNs = child.Parent?.Name.NamespaceName;

                    if (childName.StartsWith(attrChildPrefix) && child.Name.NamespaceName == parentNs) {
                        var attrName = childName.Substring(attrChildPrefix.Length);
                        ConvertElementAttribute(sb, indent, attrName, child, isSimpleNode);
                    } else if (localName.Contains(".")) {
                        ConvertElementAttribute(sb, indent, localName, child, isSimpleNode);
                    } else {
                        ConvertElement(child, sb, indent + _indentIncr);
                    }
                }
            }
        }

        private static XAttribute FindAttribute(XElement el, string xPrefix, string name)
        {
            return el.Attributes()
                .FirstOrDefault(a => a.Name.LocalName == name && 
                                     a.Name.NamespaceName == xPrefix);
        }

        private void ConvertElementAttribute(StringBuilder sb, string indent, string attrName, XElement child, bool isSimpleNode)
        {
            sb.AppendLine();
            sb.Append(indent + _indentIncr);
            sb.Append(attrName);
            sb.Append(": ");

            var values = child.Elements().ToList();
            if (values.Count > 1) {
                sb.Append("[");
                foreach (var value in values)
                    ConvertElement(value, sb, indent + _indentIncr + _indentIncr);
                sb.Append(Environment.NewLine);
                sb.Append(indent + _indentIncr);
                sb.Append("]");
            }
            else if (values.Count == 1) {
                ConvertElement(values[0], sb, indent + _indentIncr, true);
            }
            else {
                sb.Append(child.Value);
            }
        }

        private bool IsNonStandardNamespace(string localName)
        {
            return !(localName == "xmlns" || localName == "x" || localName == "mc" || localName == "d" || localName == "local");
        }
    }
}