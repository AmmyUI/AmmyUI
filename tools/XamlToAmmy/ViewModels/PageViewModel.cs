using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using PropertyChanged;

namespace XamlToAmmy.ViewModels
{
    [ImplementPropertyChanged]
    class PageViewModel
    {
        public XElement Element { get; set; }
        public XAttribute IncludeAttribute { get; set; }
        public string Filename { get; set; }
        public string FilePath { get; set; }
        public string Ammy { get; set; }
        public string ConversionStatus { get; set; }
        public bool NeedToConvert { get; set; }

        public PageViewModel(XElement el, XAttribute includeAttribute, string projectDir)
        {
            Element = el;
            NeedToConvert = true;
            IncludeAttribute = includeAttribute;
            Filename = includeAttribute.Value;
            FilePath = Path.Combine(projectDir, Filename);
        }
    }
}