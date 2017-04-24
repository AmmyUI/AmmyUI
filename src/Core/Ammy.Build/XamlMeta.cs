using System;
using System.Collections.Generic;

namespace Ammy.Build
{
    [Serializable]
    public class XamlProjectMeta
    {
        public List<XamlFileMeta> Files { get; } = new List<XamlFileMeta>();
    }

    [Serializable]
    public class XamlFileMeta
    {
        public string FilePath { get; set; }
        public string Filename { get; set; }
        public string Hash { get; set; }
        public List<XamlPropertyMeta> Properties { get; set; } = new List<XamlPropertyMeta>();
    }
    
    [Serializable]
    public class XamlPropertyMeta
    {
        public PropertyType PropertyType { get; set; }
        public string FullName { get; set; }
    }

    public enum PropertyType { DependencyProperty, RoutedEvent, Property, Event }
}