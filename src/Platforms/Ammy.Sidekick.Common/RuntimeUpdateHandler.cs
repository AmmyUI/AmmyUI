using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AmmySidekick
{
    public class RuntimeUpdateHandler
    {
        public static string CurrentlyUpdatedTargetId { get; private set; }

        static readonly HashSet<object> RegisteredElements = new HashSet<object>();
        static readonly ConcurrentDictionary<object, object> ChildToParent = new ConcurrentDictionary<object, object>();
        static readonly ConcurrentDictionary<object, string> ObjectToTarget = new ConcurrentDictionary<object, string>();
        static readonly ConcurrentDictionary<string, HashSet<object>> Nodes = new ConcurrentDictionary<string, HashSet<object>>();
        static readonly ConcurrentDictionary<string, byte[]> Buffers = new ConcurrentDictionary<string, byte[]>();
        static readonly ConcurrentDictionary<string, string> InitialPropertyList = new ConcurrentDictionary<string, string>();

        public static bool IsRegistered(string id)
        {
            return Nodes.ContainsKey(id);
        }

        public static void Register(object fe, string id)
        {
            //TODO: remove old elements with the same ID

            var nodeBag = Nodes.GetOrAdd(id, _ => new HashSet<object>());
            nodeBag.Add(fe);

            ObjectToTarget[fe] = id;
            RegisteredElements.Add(fe);

            PopulateParents(fe);

            Debug.WriteLine("Registered '" + id + "' as " + fe);
        }

        private static void PopulateParents(object root)
        {
            var fe = root as FrameworkElement;
            if (fe == null)
                return;

            var child = fe;
            var parent = fe.Parent as FrameworkElement;

            while (parent != null) {
                if (ChildToParent.ContainsKey(child))
                    break;

                ChildToParent[child] = parent;
                child = parent;
                parent = child.Parent as FrameworkElement;
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

                if (!targetId.EndsWith(".fun.xaml", StringComparison.InvariantCultureIgnoreCase))
                    pageMessages.Add(message);
            }

            var affectedObjects = pageMessages.Where(page => Nodes.ContainsKey(page.TargetId))
                                                                  .Select(page => Nodes[page.TargetId])
                                                                  .ToList();
            var roots = FindRootsOnly(affectedObjects.SelectMany(b => b).ToList());
            var timer = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(0.1)
            };

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

                var fe = rootElement as FrameworkElement;

                if (fe != null)
                    fe.SetValue(Control.BackgroundProperty, ConstructionBackground.Value);

                EventHandler timerOnTick = null;
                timerOnTick = (sender, args) => {
                    timer.Tick -= timerOnTick;
                    if (fe != null)
                        fe.ClearValue(Control.BackgroundProperty);
                    XamlHelper.InitializeComponent(rootElement, pageMessage.Buffer, new Uri(pageMessage.TargetId, UriKind.Relative));
                };
                timer.Tick += timerOnTick;
            }
            timer.Start();
        }

        private static void UnregisterChildren(object root)
        {
            var dpo = root as DependencyObject;
            if (dpo != null) {
                foreach (var child in GetLogicalDescendants(dpo)) {
                    if (RegisteredElements.Contains(child)) {
                        string targetId;

                        if (ObjectToTarget.TryGetValue(child, out targetId)) {
                            HashSet<object> set;
                            if (Nodes.TryGetValue(targetId, out set))
                                set.Remove(child);

                            string temp;
                            ObjectToTarget.TryRemove(child, out temp);
                            object tempParent;
                            ChildToParent.TryRemove(child, out tempParent);
                        }
                        
                        RegisteredElements.Remove(child);
                    }
                }
            }
        }

        public static void ClearElement(object el, string targetId, string propertyList)
        {
            ClearChildren(el);

            var frameworkElement = el as FrameworkElement;
            if (frameworkElement != null) {
                var ns = NameScope.GetNameScope(frameworkElement) as IDictionary<string, object>;
                if (ns != null)
                    ns.Clear();
            }

            if (propertyList != null)
                ResetProperties(el, targetId, propertyList);
        }

        public static void ClearChildren(object rootElement)
        {
            var decorator = rootElement as Decorator;
            var panel = rootElement as Panel;
            var contentControl = rootElement as ContentControl;
            var itemsControl = rootElement as ItemsControl;
            var fe = rootElement as FrameworkElement;
            var app = rootElement as Application;
            
            if (contentControl != null) {
                contentControl.Content = null;
            } else if (panel != null) {
                panel.Children.Clear();
            } else if (decorator != null) {
                decorator.Child = null;
            } else if (itemsControl != null && itemsControl.ItemsSource == null) {
                itemsControl.Items.Clear();
            } else if (itemsControl != null && itemsControl.ItemsSource != null) {
                itemsControl.ItemsSource = null;
            }

            if (fe != null)
                fe.Resources = null;
            if (app != null)
                app.Resources = null;
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
            var frameworkElement = rootElement as FrameworkElement;
            if (frameworkElement == null)
                return;

            InitialPropertyList.TryAdd(targetId, propertyList);

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

                    var propertyOwnerType = KnownTypes.FindType(propertyOwner, rootElement);

                    if (typePart == "DependencyProperty") {
                        var dpField = propertyOwnerType.GetField(propertyName, BindingFlags.Static | BindingFlags.Public);
                        if (dpField == null) {
                            Debug.WriteLine("Dependency property " + propertyName + " not found on type " + propertyOwnerType.FullName);
                            continue;
                        }

                        var dp = (DependencyProperty)dpField.GetValue(null);
                        frameworkElement.ClearValue(dp);
                    } else if (typePart == "RoutedEvent") {
                        var reField = propertyOwnerType.GetField(propertyName, BindingFlags.Static | BindingFlags.Public);
                        if (reField == null) {
                            Debug.WriteLine("Routed event " + propertyName + " not found on type " + propertyOwnerType.FullName);
                            continue;
                        }

                        var re = (DependencyProperty)reField.GetValue(null);
                        frameworkElement.ClearValue(re);
                    } else if (typePart == "Property") {
                        Debug.WriteLine("Resetting CLR properties not implented yet (" + propertyName + " at " + propertyOwnerType.FullName + ")");
                    } else if (typePart == "Event") {
                        Debug.WriteLine("Resetting CLR events not implented yet (" + propertyName + " at " + propertyOwnerType.FullName + ")");
                    }
                } else {
                    
                }
            }
        }

        private static HashSet<object> FindRootsOnly(IList<object> affectedElements)
        {
            var roots = new HashSet<object>();

            foreach (var element in affectedElements) {
                var fe = element as FrameworkElement;
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

        private static IList<FrameworkElement> GetAncestors(FrameworkElement element)
        {
            var result = new List<FrameworkElement>();
            var parent = element.Parent as FrameworkElement;

            while (parent != null) {
                result.Add(parent);
                var child = parent;
                parent = child.Parent as FrameworkElement;
            }

            return result;
        }
        
        private static Lazy<Brush> ConstructionBackground
        {
            get {
                return new Lazy<Brush>(() => 
                    new VisualBrush {
                        TileMode = TileMode.Tile,
                        Viewport = new Rect(0, 0, 25, 25),
                        ViewportUnits = BrushMappingMode.Absolute,
                        Visual = new Border {
                                Background = Brushes.Black,
                                Child = new Grid {
                                    Opacity = 0.65,
                                    Background = Brushes.Yellow,
                                    Width = 100,
                                    Height = 100,
                                    ClipToBounds = true,
                                    Children = {
                                    new Rectangle {
                                        Width = 40,
                                        Height = 100,
                                        RenderTransformOrigin = new Point(0, 0.25),
                                        Fill = Brushes.Black,
                                        RenderTransform = new SkewTransform(45, 0)
                                    },
                                    new Rectangle {
                                        Margin = new Thickness(-200, 0, 0, 0),
                                        Width = 40,
                                        Height = 100,
                                        RenderTransformOrigin = new Point(0, 0.25),
                                        Fill = Brushes.Black,
                                        RenderTransform = new SkewTransform(45, 0)
                                    }
                                }
                            }
                        }
                    }
                );
            }
        }

        private static IEnumerable<DependencyObject> GetLogicalDescendants(DependencyObject parent)
        {
            foreach (object child in LogicalTreeHelper.GetChildren(parent)) {
                var dependencyObject = child as DependencyObject;
                if (dependencyObject != null) {
                    
                    yield return dependencyObject;

                    foreach (var grandChild in GetLogicalDescendants(dependencyObject))
                        yield return grandChild;
                }
            }
        }
    }
}