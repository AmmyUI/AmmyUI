using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

// ReSharper disable once CheckNamespace
namespace AmmySidekick
{
    public class ExpressionBuilder
    {
        public HashSet<string> UsedTypes { get; private set; }
        
        private readonly object _instance;
        private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();

        public ExpressionBuilder(object instance)
        {
            UsedTypes = new HashSet<string>();
            _instance = instance;
        }
        
        public LambdaExpression Build(XDocument doc)
        {
            if (doc.Root == null) return Expression.Lambda(Expression.Empty());

            var deserialized = doc.Root.Elements()
                                  .Select(DeserializeNode)
                                  .ToList();

            if (_variables.Count > 0)
                return Expression.Lambda(
                    Expression.Block(_variables, deserialized));
            
            if (deserialized.Count == 0)
                return Expression.Lambda(Expression.Empty());

            if (deserialized.Count == 1 && deserialized[0] is LambdaExpression)
                return (LambdaExpression)deserialized[0];

            return Expression.Lambda(Expression.Block(deserialized));
        }

        private ParameterExpression FindVariable(string name)
        {
            return _variables.First(p => p.Name == name);
        }

        private Expression DeserializeNode(XElement el)
        {
            /*
               | Seq(elems) => string.Join(Environment.NewLine, elems.Select(ast => InitAstToXml(ast)))
               | Variable(name) => $"<var name=\"$name\" />"
               | NewVariable(name) => $"<newvar name=\"$name\" />"
               | New(ctor, parms) => 
                 def parmString = string.Join("", parms.Select(p => InitAstToXml(p)));
                 $"<new ctor=\"$ctor\">$parmString</new>"
               | PrimitiveValue(type, val) => 
                 def val = SecurityElement.Escape(val);
                 $"<prim type=\"$type\" val=\"$val\" />"
               | Assign(left, right) => "<assign>" + InitAstToXml(left) + InitAstToXml(right) + "</assign>"
               | Property(instance, propName) => $"<prop name=\"$propName\">" + InitAstToXml(instance) + "</prop>"
               | Call(left, method, parms) => 
                 def parmString = string.Join("", parms.Select(p => InitAstToXml(p)));
                 $"<call method=\"$method\">" + InitAstToXml(left) + parmString + "</call>"
               | This => "<this />"
            */
            var cmd = el.Name.LocalName;

            if (cmd == "var") return DeserializeVar(el);
            else if (cmd == "newvar") return DeserializeNewVar(el);
            else if (cmd == "new") return DeserializeNew(el);
            else if (cmd == "prim") return DeserializePrimitive(el);
            else if (cmd == "assign") return DeserializeAssign(el);
            else if (cmd == "prop") return DeserializeProperty(el);
            else if (cmd == "field") return DeserializeField(el);
            else if (cmd == "call") return DeserializeCall(el);
            else if (cmd == "this") return DeserializeThis(el);
            else if (cmd == "null") return DeserializeNull(el);
            else if (cmd == "cast") return DeserializeCast(el);
            else if (cmd == "staticcall") return DeserializeStaticCall(el);
            else if (cmd == "staticfield") return DeserializeStaticField(el);
            else if (cmd == "staticproperty") return DeserializeStaticProperty(el);
            else if (cmd == "typeof") return DeserializeTypeof(el);
            else if (cmd == "binary") return DeserializeBinary(el);
            else if (cmd == "unary") return DeserializeUnary(el);
            else if (cmd == "lambda") return DeserializeLambda(el);
            else if (cmd == "parameter") return DeserializeParameter(el);
            else if (cmd == "delegate") return DeserializeDelegate(el);
            else if (cmd == "ternary") return DeserializeTernary(el);
            else if (cmd == "arrayaccess") return DeserializeArrayAccess(el);

            throw new ArgumentException("Invalid serialized line command: " + cmd);
        }

        private MethodInfo DeserializeMethodInfo(XElement el)
        {
            var elements = el.Elements().ToArray();
            var methodName = el.Attribute("name").Value;
            var isInstance = el.Attribute("isInstance").Value;
            var bindingFlags = isInstance == "true" 
                               ? BindingFlags.Public | BindingFlags.Instance
                               : BindingFlags.Public | BindingFlags.Static;

            if (elements[0].Name == "typeinfo") {
                var ownerType = DeserializeTypeInfo(elements[0]);
                return ownerType.GetMethod(methodName, bindingFlags);
            } else {
                var ownerNode = DeserializeNode(elements[0]);
                return ownerNode.Type.GetMethod(methodName, bindingFlags);
            }
        }

        // Simplified version that doesn't check for delegate type while selecting method
        // Also doesn't allow having method instance other than root owner
        private Expression DeserializeDelegate(XElement el)
        {
            var delegateType = el.Elements("typeinfo")
                                 .Select(DeserializeTypeInfo)
                                 .FirstOrDefault();
            var methodInfo = el.Elements("methodinfo")
                               .Select(DeserializeMethodInfo)
                               .FirstOrDefault();

            if (delegateType == null)
                throw new InvalidOperationException("Delegate type is missing from DeserializeDelegate");

            if (methodInfo == null)
                throw new InvalidOperationException("MethodInfo is missing from DeserializeDelegate");


#if WINDOWS_UWP || WINDOWS_PHONE_APP
            return Expression.Constant(methodInfo.CreateDelegate(delegateType));
#else
            return Expression.Constant(Delegate.CreateDelegate(delegateType, methodInfo));
#endif
        }

        private Expression DeserializeEventAdd(XElement el)
        {
            var elements = el.Elements().ToArray();

            var eventOwner = DeserializeNode(elements[0]);
            var eventType = DeserializeTypeInfo(elements[1]);

            var eventName = el.Attribute("eventname").Value;
            var eventHandlerName = el.Attribute("handler").Value;
            
            var createDelegateMethod = typeof(MethodInfo).GetMethod("CreateDelegate", new[] { typeof(Type), typeof(object) });
            var eventHandler = Expression.Constant(_instance.GetType().GetMethod(eventHandlerName));
            var createDelegateCall = Expression.Call(eventHandler,
                                                     createDelegateMethod,
                                                     Expression.Constant(eventType),
                                                     eventOwner);
            var convertToHandler = Expression.Convert(createDelegateCall, eventType);
            
            return Expression.Call(eventOwner, "add_" + eventName, null, convertToHandler);
        }

        private Type DeserializeTypeInfo(XElement el)
        {
            var typename = el.Attribute("typename").Value;
            var isArray = el.Attribute("isarray").Value == bool.TrueString;

            var genericArgs = el.Elements().Select(DeserializeTypeInfo).ToArray();

            if (genericArgs.Length > 0) {
                // Remove <T, ...> at the end
                typename = Regex.Replace(typename, @"<(\w|\s|,)+>$", "");
                var type = KnownTypes.FindType(typename + "`" + genericArgs.Length, _instance);
                return type.MakeGenericType(genericArgs);
            }

            var resultType = KnownTypes.FindType(typename, _instance);

            if (!isArray)
                return resultType;
            return 
                resultType.MakeArrayType();
        }

        readonly Stack<ParameterExpression> _lambdaParameterStack = new Stack<ParameterExpression>();

        private Expression DeserializeLambda(XElement el)
        {
            var isAction = el.Attribute("isaction").Value == bool.TrueString;
            var elements = el.Elements().ToArray();
            var lambdaParameters = elements.Skip(1).Select(DeserializeNode).OfType<ParameterExpression>().ToArray();

            foreach (var parameterExpression in lambdaParameters)
                _lambdaParameterStack.Push(parameterExpression);

            var body = isAction 
                       ? Expression.Block(DeserializeNode(elements[0]), Expression.Empty())
                       : DeserializeNode(elements[0]);

            foreach (var _ in lambdaParameters)
                _lambdaParameterStack.Pop();

            return Expression.Lambda(body, lambdaParameters);
        }
        
        private ParameterExpression DeserializeParameter(XElement el)
        {
            var name = el.Attribute("name").Value;
            var type = el.Attribute("type").Value;

            var parameterOnStack = _lambdaParameterStack.FirstOrDefault(p => p.Name == name);
            return parameterOnStack ?? Expression.Parameter(KnownTypes.FindType(type, _instance), name);
        }
        
        private Expression DeserializeUnary(XElement el)
        {
            var expr = el.Elements().Select(DeserializeNode).ToArray();
            var op = el.Attribute("op").Value;

            Debug.Assert(expr.Length == 1, "expr.Length != 1" + Environment.NewLine + el);

            if (op == "Minus")
                return Expression.Subtract(Expression.Constant(0), expr[0]);
            if (op == "LogicalNegate")
                return Expression.Not(expr[0]);

            throw new NotImplementedException();
        }

        private Expression DeserializeBinary(XElement el)
        {
            var leftRight = el.Elements().Select(DeserializeNode).ToArray();
            var op = el.Attribute("op").Value;

            Debug.Assert(leftRight.Length == 2, "leftRight.Length != 2" + Environment.NewLine + el);

            if (op == "Or")
                return Expression.Or(leftRight[0], leftRight[1]);
            if (op == "And")
                return Expression.And(leftRight[0], leftRight[1]);
            if (op == "Equal")
                return Expression.Equal(leftRight[0], leftRight[1]);
            if (op == "NotEqual")
                return Expression.NotEqual(leftRight[0], leftRight[1]);
            if (op == "LessEqual")
                return Expression.LessThanOrEqual(leftRight[0], leftRight[1]);
            if (op == "Less")
                return Expression.LessThan(leftRight[0], leftRight[1]);
            if (op == "GreaterEqual")
                return Expression.GreaterThanOrEqual(leftRight[0], leftRight[1]);
            if (op == "Greater")
                return Expression.GreaterThan(leftRight[0], leftRight[1]);
            if (op == "Sum") {
                if (leftRight[0].Type == typeof(string) && leftRight[1].Type == typeof(string)) {
                    var concat = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
                    return Expression.Call(concat, leftRight[0], leftRight[1]);
                }

                if (leftRight[0].Type == typeof (string) || leftRight[1].Type == typeof (string)) {
                    var concat = typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) });
                    return Expression.Call(concat, Expression.Convert(leftRight[0], typeof(object)), Expression.Convert(leftRight[1], typeof(object)));
                }

                return Expression.Add(leftRight[0], leftRight[1]);
            }
            if (op == "Sub")
                return Expression.Subtract(leftRight[0], leftRight[1]);
            if (op == "Mod")
                return Expression.Modulo(leftRight[0], leftRight[1]);
            if (op == "Mul")
                return Expression.Multiply(leftRight[0], leftRight[1]);
            if (op == "Div")
                return Expression.Divide(leftRight[0], leftRight[1]);

            throw new NotImplementedException();
        }

        private Expression DeserializeTernary(XElement el)
        {
            var nodes = el.Elements().Select(DeserializeNode).ToArray();

            if (nodes.Length != 3)
                throw new Exception("Invalid ternary operator definition. Node count should be 3, but actually is " + nodes.Length);

            return Expression.Condition(nodes[0], nodes[1], nodes[2]);
        }

        private Expression DeserializeTypeof(XElement el)
        {
            var type = el.Elements("typeinfo")
                         .Select(DeserializeTypeInfo)
                         .FirstOrDefault();
            var helpersType = typeof (KnownTypes);
            var typeofMethod = helpersType.GetMethod("FindType", new Type[0])
                                          .MakeGenericMethod(type);

            return Expression.Call(typeofMethod);
        }

        private Expression DeserializeThis(XElement el)
        {
            return Expression.Constant(_instance);
        }

        private Expression DeserializeNull(XElement el)
        {
            var type = DeserializeTypeInfo(el.Element("typeinfo"));
            return Expression.Constant(null, type);
        }

        private Expression DeserializeCast(XElement el)
        {
            var elements = el.Elements().ToArray();
            var type = DeserializeTypeInfo(elements[0]);
            var obj = DeserializeNode(elements[1]);

            return Expression.Convert(obj, type);
        }
        
        private Expression DeserializeStaticField(XElement el)
        {
            var fieldName = el.Attribute("field").Value;
            var type = DeserializeTypeInfo(el.Element("typeinfo"));

            return Expression.Field(null, type, fieldName);
        }

        private Expression DeserializeStaticProperty(XElement el)
        {
            var propertyName = el.Attribute("property").Value;
            var type = DeserializeTypeInfo(el.Element("typeinfo"));

            return Expression.Property(null, type, propertyName);
        }

        private Expression DeserializeCall(XElement el)
        {
            var methodName = el.Attribute("method").Value;
            var children = el.Elements().Select(DeserializeNode).ToArray();

            if (children.Length < 1)
                throw new Exception("Invalid Call format, no instance provided. " + el);

            var instance = children[0];
            var parameters = children.Skip(1).ToArray();
            /*Type instanceType;

            if (instance is ConstantExpression)
                instanceType = ((ConstantExpression) instance).Type;
            else throw new InvalidOperationException("Couldn't find instance type for " + instance);

            var addMethod = instanceType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
            */

            var method = instance.Type.GetMethod(methodName, parameters.Select(p => p.Type).ToArray());
            var methodParameterTypes = method.GetParameters().Select(pi => pi.ParameterType).ToArray();
            var convertedParameters = parameters.Select((p, i) => methodParameterTypes[i] == p.Type ? p : Expression.Convert(p, methodParameterTypes[i])).ToArray();

            return Expression.Call(instance, method, convertedParameters);
        }

        private Expression DeserializeStaticCall(XElement el)
        {
            var type = el.Elements("typeinfo")
                         .Select(DeserializeTypeInfo)
                         .FirstOrDefault();
            var methodName = el.Attribute("method").Value;
            var children = el.Elements().Skip(1).Select(DeserializeNode).ToArray();
            var parameters = children.ToArray();
            var method = type.GetMethod(methodName, parameters.Select(p => p.Type).ToArray());
            var methodParameterTypes = method.GetParameters().Select(pi => pi.ParameterType).ToArray();
            var convertedParameters = parameters.Select((p, i) => methodParameterTypes[i] == p.Type ? p : Expression.Convert(p, methodParameterTypes[i])).ToArray();

            return Expression.Call(method, convertedParameters);
        }

        private Expression DeserializeProperty(XElement el)
        {
            var propName = el.Attribute("name").Value;
            var instance = el.Elements().Select(DeserializeNode).First();

            return Expression.Property(instance, propName);
        }

        private Expression DeserializeField(XElement el)
        {
            var propName = el.Attribute("name").Value;
            var instance = el.Elements().Select(DeserializeNode).First();

            return Expression.Field(instance, propName);
        }

        private Expression DeserializeAssign(XElement el)
        {
            var leftRight = el.Elements().Select(DeserializeNode).ToArray();

            Debug.Assert(leftRight.Length == 2, "leftRight.Length != 2" + Environment.NewLine + el);

            return Expression.Assign(leftRight[0], leftRight[1]);
        }

        private Expression DeserializePrimitive(XElement el)
        {
            var val = el.Attribute("val").Value;
            var isnull = el.Attribute("isnull").Value;
            
            var elements = el.Elements().ToArray();

            if (elements.Length != 1)
                throw new Exception("Invalid primitive value declaration");

            var type = DeserializeTypeInfo(elements[0]);
            
            if (NumericTypes.TypeIsNumeric(type)) {
                decimal res;
                if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out res))
                    return Expression.Constant(Convert.ChangeType(res, type, null));
                throw new FormatException("Couldn't parse number '" + val + "' typed as " + type.Name);
            }

#if WINDOWS_UWP || WINDOWS_PHONE_APP
            if (type.GetTypeInfo().IsEnum)
                return Expression.Constant(Enum.Parse(type, val.Substring(val.LastIndexOf(".") + 1)));
#else
            if (type.IsEnum)
                return Expression.Constant(Enum.Parse(type, val.Substring(val.LastIndexOf(".") + 1), true));
#endif
            if (isnull == "True")
                return Expression.Constant(null, type);

            if (type == typeof(bool))
                return Expression.Constant(bool.Parse(val), type);

            if (type == typeof (char) && val.Length >= 0)
                return Expression.Constant(val[0], type);

            return Expression.Constant(val, type);
        }

        private Expression DeserializeNewVar(XElement el)
        {
            var name = el.Attribute("name").Value;
            var type = el.Elements("typeinfo")
                         .Select(DeserializeTypeInfo)
                         .ToArray();

            Debug.Assert(type.Length == 1);

            var variable = Expression.Variable(type[0], name);

            _variables.Add(variable);

            return variable;
        }

        private Expression DeserializeVar(XElement el)
        {
            var name = el.Attribute("name").Value;
            return FindVariable(name);
        }

        private Expression DeserializeArrayAccess(XElement el)
        {
            var nodes = el.Elements().Select(DeserializeNode).ToArray();

            if (nodes.Length != 2)
                throw new Exception("Invalid ArrayAccess declaration");

            if (nodes[0].Type.IsArray)
                return Expression.ArrayAccess(nodes[0], nodes[1]);

            var indexer = nodes[0].Type.GetDefaultMembers().OfType<PropertyInfo>()
                                    .FirstOrDefault(pi => {
                                        var indexerParms = pi.GetIndexParameters();
                                        if (indexerParms.Length == 1 && indexerParms[0].ParameterType.IsAssignableFrom(nodes[1].Type))
                                            return true;
                                        return false;
                                    });
            if (indexer == null)
                throw new Exception("Indexer with parameter type " + nodes[1].Type + " not found on type " + nodes[0].Type);

            return Expression.Property(nodes[0], indexer, nodes[1]);
        }

        private Expression DeserializeNew(XElement el)
        {
            var elementArray = el.Elements().ToArray();
            var type = DeserializeTypeInfo(elementArray[0]);
            
            UsedTypes.Add(type.FullName);

            var parms = elementArray.Skip(1)
                                    .Select(DeserializeNode)
                                    .ToArray();
            var parmTypes = parms.Select(c => c.Type)
                                 .ToArray();

            // Value types don't have parameterless constructors
#if WINDOWS_UWP || WINDOWS_PHONE_APP
            if (type.GetTypeInfo().IsValueType && parms.Length == 0)
                return Expression.New(type);
#else
            if (type.IsValueType && parms.Length == 0)
                return Expression.New(type);
#endif

            return Expression.New(KnownTypes.GetConstructor(type, parmTypes), parms);
        }
    }
}
