using System;
using System.Linq;
using System.Text;
using Ammy.Build;
using Ammy.Infrastructure;
using Ammy.Language;
using Ammy.VisualStudio.Service.Settings;

namespace Ammy.VisualStudio.Service
{
    public class Formatting
    {
        private readonly char[] _s = { ' ', '\r', '\n' };
        private readonly string _indentIncr;

        public Formatting(string indentIncr)
        {
            _indentIncr = indentIncr;
        }

        public string FormatFile(AmmyFile<Top> file)
        {
            var sb = new StringBuilder();
            var top = ((Start)file.Ast).Top;

            FormatUsings(top.Usings, sb, "");
            FormatGlobalDeclarations(top.GlobalDeclarations, sb, "");

            if (top is TopWithNode)
                FormatTopWithNode((TopWithNode) top, sb, "");
            
            return sb.ToString();
        }
        
        private void FormatUsings(SyntaxModuleUsingDirective.IAstList usings, StringBuilder sb, string indent)
        {
            foreach (var @using in usings)
                sb.AppendLine(indent + @using.Location.GetText().TrimEnd(_s));

            if (usings.Any())
                sb.AppendLine();
        }
        

        private void FormatTopWithNode(TopWithNode top, StringBuilder sb, string indent)
        {
            FormatNode(top.TopNode, sb, indent, true, true);
        }

        private void FormatGlobalDeclarations(GlobalDeclaration.IAstList globalDeclarations, StringBuilder sb, string indent)
        {
            foreach (var declaration in globalDeclarations) {
                if (declaration is GlobalDeclaration.Variable) {
                    FormatVariable((GlobalDeclaration.Variable) declaration, sb, indent);
                } else if (declaration is GlobalDeclaration.ContentFunction) {
                    FormatContentFunction((GlobalDeclaration.ContentFunction) declaration, sb, indent);
                } else if (declaration is GlobalDeclaration.TypeFunction) {
                    FormatTypeFunction((GlobalDeclaration.TypeFunction) declaration, sb, indent);
                }
            }

            if (globalDeclarations.Any())
                sb.AppendLine();
        }

        private void FormatVariable(GlobalDeclaration.Variable variable, StringBuilder sb, string indent)
        {
            sb.AppendLine(indent + variable.Location.GetText().TrimEnd(_s));
        }

        private void FormatContentFunction(GlobalDeclaration.ContentFunction function, StringBuilder sb, string indent)
        {
            var text = function.Location.GetText();

            sb.Append(indent + GetHeader(text));

            FormatOpeningBrace(function.Members, sb, indent, true);

            FormatMembers(function.Members, sb, indent + _indentIncr, true);

            FormatClosingBrace(function.Members, sb, indent, true);
        }

        private static void FormatOpeningBrace(NodeMember.IAstList members, StringBuilder sb, string indent, bool isMultiLineNode)
        {
            if (!isMultiLineNode) {
                sb.Append(" { ");
            } else if (AmmySettings.OpeningBraceOnSameLine) {
                sb.Append(" {" + Environment.NewLine);
            } else {
                sb.Append(Environment.NewLine + indent + "{" + Environment.NewLine);
            }
        }

        private static void FormatClosingBrace(NodeMember.IAstList members, StringBuilder sb, string indent, bool isMultiLineNode)
        {
            if (!isMultiLineNode) {
                sb.Append(" }");
            } else {
                sb.Append(indent + "}");
            }
        }

        private void FormatMembers(NodeMember.IAstList members, StringBuilder sb, string indent, bool isMultiLineNode)
        {
            foreach (var member in members) {
                FormatMember(member, sb, isMultiLineNode ? indent : "");
                
                if (isMultiLineNode)
                    sb.AppendLine();
            }
        }

        private void FormatMember(NodeMember member, StringBuilder sb, string indent)
        {
            if (member is Node) {
                var node = (Node) member;
                var isMultilineNode = node.Declarations.Any() || node.Members.Count() > 1;
                FormatNode(node, sb, indent, true, isMultilineNode);
            } else if (member is Property) {
                FormatProperty((Property) member, sb, indent);
            } else if (member is TypelessProperty) {
                FormatTypelessProperty((TypelessProperty) member, sb, indent);
            } else if (member is StyleSetters) {
                FormatStyleSetters((StyleSetters) member, sb, indent);
            } else if (member is ContentFunctionRef) {
                FormatContentFunctionRef((ContentFunctionRef)member, sb, indent);
            } else if (member is TypeFunctionRef) {
                FormatTypeFunctionRef((TypeFunctionRef) member, sb, indent);
            } else if (member is StringContent) {
                FormatStringContent((StringContent)member, sb, indent);
            }
        }

        private void FormatStringContent(StringContent stringContent, StringBuilder sb, string indent)
        {
            sb.Append(indent + "\"" + stringContent.Val.Value + "\"");
        }

        private void FormatTypeFunctionRef(TypeFunctionRef typeFunctionRef, StringBuilder sb, string indent, bool insertInitialIndent = true)
        {
            var text = typeFunctionRef.Location.GetText();
            var initialIndent = insertInitialIndent ? indent : " ";

            sb.Append(initialIndent + GetHeader(text));

            FormatNodeBody(typeFunctionRef, sb, indent, true);
        }

        private void FormatContentFunctionRef(ContentFunctionRef function, StringBuilder sb, string indent)
        {
            var text = function.Location.GetText().TrimEnd(_s);
            sb.Append(indent + text);
        }

        private void FormatStyleSetters(StyleSetters styleSetters, StringBuilder sb, string indent)
        {
            sb.Append("// Style setters formatting not supported yet");
        }

        private void FormatTypelessProperty(TypelessProperty typelessProperty, StringBuilder sb, string indent)
        {
            sb.Append(indent + typelessProperty.PropertyName.Value + ": " + typelessProperty.PropertyValue.Value);
        }

        private void FormatProperty(Property property, StringBuilder sb, string indent)
        {
            var combine = property.IsCombine.Value ? "combine " : "";
            var keyName = property.Key.FullName();
            sb.Append(indent + combine + keyName + ": ");
            FormatPropertyValue(property.Val, sb, indent);
        }

        private void FormatPropertyValue(PropertyValue val, StringBuilder sb, string indent, bool isInsideArray = false)
        {
            if (val is PropertyValue.NodeValue) {
                var nodeValue = (PropertyValue.NodeValue) val;
                var combine = nodeValue.IsCombine.Value ? "combine " : "";

                sb.Append(combine);

                FormatNode(nodeValue.Node, sb, indent, isInsideArray);
            } else if (val is PropertyValue.TypeFunction) {
                var typeFunction = (PropertyValue.TypeFunction) val;
                FormatTypeFunctionRef(typeFunction.TypeFunction, sb, indent);
            } else if (val is PropertyValue.ValueList) {
                if (val.Location.EndLineColumn.Line == val.Location.StartLineColumn.Line) {
                    sb.Append(val.Location.GetText().TrimEnd(_s));
                } else {
                    var valueList = (PropertyValue.ValueList) val;
                    sb.AppendLine("[");

                    foreach (var value in valueList.Values) {
                        FormatPropertyValue(value, sb, indent + _indentIncr, true);
                        sb.AppendLine();
                    }

                    sb.Append(indent + "]");
                }
            } else {
                var text = val.Location.GetText().TrimEnd(_s);
                sb.Append(text);
            }
        }

        private void FormatNode(NodeBase node, StringBuilder sb, string indent, bool insertInitialIndent = true, bool isMultiLineNode = true)
        {
            var text = node.Location.GetText();
            var initialIndent = insertInitialIndent ? indent : "";

            sb.Append(initialIndent + GetHeader(text));

            FormatNodeBody(node, sb, indent, isMultiLineNode);
        }

        private void FormatNodeBody(NodeBase node, StringBuilder sb, string indent, bool isMultiLineNode)
        {
            FormatOpeningBrace(node.Members, sb, indent, isMultiLineNode);

            FormatGlobalDeclarations(node.Declarations, sb, indent + _indentIncr);
            FormatMembers(node.Members, sb, indent + _indentIncr, isMultiLineNode);

            FormatClosingBrace(node.Members, sb, indent, isMultiLineNode);
        }

        private void FormatTypeFunction(GlobalDeclaration.TypeFunction typeFunction, StringBuilder sb, string indent)
        {
            var text = typeFunction.Location.GetText();
            var header = GetHeader(text);

            sb.Append(indent + header);

            FormatOpeningBrace(new NodeMember.AstList(typeFunction), sb, indent, true);

            var content = typeFunction.Content;
            
            if (content is TypeFunctionContent.NodeContent) {
                var nodeContent = (TypeFunctionContent.NodeContent) content;
                FormatNode(nodeContent.Node, sb, indent + _indentIncr);
            } else if (content is TypeFunctionContent.TypeFunctionRefContent) {
                var typeFunctionRefContent = (TypeFunctionContent.TypeFunctionRefContent) content;
                FormatTypeFunctionRef(typeFunctionRefContent.TypeFunction, sb, indent + _indentIncr);
            }

            sb.AppendLine(indent + "}");
        }

        private string GetHeader(string headerWithBody)
        {
            var openingBraceIndex = headerWithBody.IndexOf('{');

            if (openingBraceIndex == -1)
                return "";

            return headerWithBody.Substring(0, openingBraceIndex).TrimEnd(_s);
        }
    }
}