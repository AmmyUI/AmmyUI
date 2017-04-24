using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace AmmySidekick
{
    public class RuntimeUpdateHandler
    {
        public static string CurrentlyUpdatedTargetId { get; private set; }

        static readonly HashSet<object> RegisteredElements = new HashSet<object>();
        static readonly Dictionary<object, object> ChildToParent = new Dictionary<object, object>();
        static readonly Dictionary<object, string> ObjectToTarget = new Dictionary<object, string>();
        static readonly Dictionary<string, HashSet<object>> Nodes = new Dictionary<string, HashSet<object>>();
        static readonly Dictionary<string, byte[]> Buffers = new Dictionary<string, byte[]>();
        static readonly Dictionary<string, string> InitialPropertyList = new Dictionary<string, string>();
        
        public static void Register(object fe, string id)
        {
            //TODO: remove old elements with the same ID

            HashSet<object> nodeBag;
            if (!Nodes.TryGetValue(id, out nodeBag))
                Nodes[id] = nodeBag = new HashSet<object>();

            nodeBag.Add(fe);

            ObjectToTarget[fe] = id;
            RegisteredElements.Add(fe);

            PopulateParents(fe);

            Debug.WriteLine("Registered '" + id + "' as " + fe);
        }

        private static void PopulateParents(object root)
        {
            var fe = root as Element;
            if (fe == null)
                return;

            var child = fe;
            var parent = fe.Parent;

            while (parent != null) {
                if (ChildToParent.ContainsKey(child))
                    break;

                ChildToParent[child] = parent;
                child = parent;
                parent = child.Parent;
            }
        }

        public static byte[] FindBuffer(string id)
        {
            byte[] val;
            if (Buffers.TryGetValue(id, out val))
                return val;
            return null;
        }

        public static void ReceiveMessages(List<Message> messages)
        {
            var pageMessages = new List<Message>();

            foreach (var message in messages) {
                var targetId = message.TargetId;

                Buffers[targetId] = message.Buffer;

                if (!targetId.EndsWith(".fun.xaml", StringComparison.OrdinalIgnoreCase))
                    pageMessages.Add(message);
            }

            var affectedObjects = pageMessages.Where(page => Nodes.ContainsKey(page.TargetId))
                                                                  .Select(page => Nodes[page.TargetId])
                                                                  .ToList();
            var roots = FindRootsOnly(affectedObjects.SelectMany(b => b).ToList());
            
            foreach (var root in roots) {
                var rootElement = root;
                string targetId;

                if (!ObjectToTarget.TryGetValue(rootElement, out targetId))
                    continue;
                
                var pageMessage = pageMessages.FirstOrDefault(p => p.TargetId == targetId);

                if (pageMessage == null)
                    continue;
                
                Debug.WriteLine("Updating '" + pageMessage.TargetId + "' as " + rootElement);

                UnregisterChildren(rootElement);

                CurrentlyUpdatedTargetId = pageMessage.TargetId;

                ClearElement(rootElement, pageMessage.TargetId, pageMessage.PropertyList);
                InitializeComponent(rootElement, pageMessage.Buffer);
            }
        }

        public static void InitializeComponent(object element, byte[] xaml)
        {
            try {
                var extensions = typeof (Extensions).GetTypeInfo();
                var loadXamlMethodGeneric = extensions.DeclaredMethods
                    .FirstOrDefault(mi => {
                        if (mi.Name != "LoadFromXaml")
                            return false;

                        var parameters = mi.GetParameters();

                        return parameters.Length == 2 && parameters[1].ParameterType == typeof (string);
                    });

                var loadXamlMethod = loadXamlMethodGeneric.MakeGenericMethod(typeof (object));
                var xamlString = Encoding.Unicode.GetString(xaml, 0, xaml.Length);

                loadXamlMethod.Invoke(null, new[] {element, xamlString});
            } catch (TargetInvocationException e) {
                SetExceptionContent(element, e.InnerException);
            } catch (Exception e) {
                SetExceptionContent(element, e);
            }
        }

        private static void SetExceptionContent(object rootElement, Exception e)
        {
            var message = e.Message;
            var innerException = e.InnerException;

            while (innerException != null) {
                message += Environment.NewLine + innerException.Message;
                innerException = innerException.InnerException;
            }

            var label = new Label {
                Text = message,
                TextColor = Color.Red
            };

            if (rootElement is ContentPage) {
                ((ContentPage) rootElement).Content = label;
            } else if (rootElement is ContentView) {
                ((ContentView)rootElement).Content = label;
            }
        }

        private static void UnregisterChildren(object root)
        {
            var element = root as Element;
            if (element != null) {
                foreach (var child in GetLogicalDescendants(element)) {
                    if (RegisteredElements.Contains(child)) {
                        string targetId;

                        if (ObjectToTarget.TryGetValue(child, out targetId)) {
                            HashSet<object> set;
                            if (Nodes.TryGetValue(targetId, out set))
                                set.Remove(child);
                            
                            ObjectToTarget.Remove(child);
                            ChildToParent.Remove(child);
                        }
                        
                        RegisteredElements.Remove(child);
                    }
                }
            }
        }

        public static void ClearElement(object el, string targetId, string propertyList)
        {
            ClearChildren(el);

            var frameworkElement = el as Element;
            if (frameworkElement != null) {
                var ns = NameScope.GetNameScope(frameworkElement) as NameScope;
                if (ns != null) {
                    // TODO! remove all registered elements
                }
                    
            }

            if (propertyList != null)
                ResetProperties(el, targetId, propertyList);
        }

        public static void ClearChildren(object rootElement)
        {
            if (rootElement is ContentPage) {
                ((ContentPage)rootElement).Content = null;
            } else if (rootElement is ContentView) {
                ((ContentView)rootElement).Content = null;
            }
        }

        public static string GetInitialPropertyList(string targetId)
        {
            string propertyList;
            if (InitialPropertyList.TryGetValue(targetId, out propertyList))
                return propertyList;
            return null;
        }

        private static void ResetProperties(object rootElement, string targetId, string propertyList)
        {
            var frameworkElement = rootElement as Element;
            if (frameworkElement == null)
                return;

            InitialPropertyList[targetId] = propertyList;

            foreach (var property in propertyList.Split(',')) {
                var split = property.Split('|');

                if (split.Length != 2)
                    continue;

                var typePart = split[0];
                var propertyPart = split[1];
                var lastDot = propertyPart.LastIndexOf('.');

                if (lastDot != -1) {
                    var propertyOwner = propertyPart.Substring(0, lastDot);
                    var propertyName = propertyPart.Substring(lastDot + 1);
                    var propertyOwnerType = KnownTypes.FindType(propertyOwner);

                    if (typePart == "DependencyProperty") {
                        var dpField = propertyOwnerType.GetTypeInfo().DeclaredFields.FirstOrDefault(fi => fi.Name == propertyName && fi.IsStatic && fi.IsPublic);
                        if (dpField == null) {
                            Debug.WriteLine("Dependency property " + propertyName + " not found on type " + propertyOwnerType.FullName);
                            continue;
                        }

                        var dp = (BindableProperty)dpField.GetValue(null);
                        frameworkElement.ClearValue(dp);
                    } else if (typePart == "Property") {
                        Debug.WriteLine("Resetting CLR properties not implented yet (" + propertyName + " at " + propertyOwnerType.FullName + ")");
                    } else if (typePart == "Event") {
                        Debug.WriteLine("Resetting CLR events not implented yet (" + propertyName + " at " + propertyOwnerType.FullName + ")");
                    }
                }
            }
        }

        private static HashSet<object> FindRootsOnly(IList<object> affectedElements)
        {
            var roots = new HashSet<object>();

            foreach (var element in affectedElements) {
                var fe = element as Element;
                if (fe != null) {
                    var ancestors = GetAncestors(fe);
                    var affectedAncestor =
                        ancestors.LastOrDefault(ancestor => affectedElements.Any(ae => ae != element && ae == ancestor));

                    roots.Add(affectedAncestor ?? element);
                }
                else {
                    roots.Add(element);
                }
            }

            return roots;
        }

        private static IList<Element> GetAncestors(Element element)
        {
            var result = new List<Element>();
            var parent = element.Parent;

            while (parent != null) {
                result.Add(parent);
                var child = parent;
                parent = child.Parent;
            }

            return result;
        }
        
        private static IEnumerable<Element> GetLogicalDescendants(Element parent)
        {
            var ec = (IElementController) parent;
            foreach (var child in ec.LogicalChildren) {
                yield return child;
                foreach (var grandChild in GetLogicalDescendants(child))
                    yield return grandChild;
            }
        }
    }
}