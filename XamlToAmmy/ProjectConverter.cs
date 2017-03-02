using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using XamlToAmmy.ViewModels;

namespace XamlToAmmy
{
    internal class ProjectConverter
    {
        public string ProjectDir => _projectDir;

        private List<PageViewModel> _pages;
        private XDocument _doc;
        private XNamespace _rootNs;
        private string _projectDir;
        private string _projectFilePath;

        public IReadOnlyList<PageViewModel> LoadProject(string projectFile)
        {
            _projectFilePath = projectFile;
            _doc = XDocument.Parse(File.ReadAllText(projectFile));
            _projectDir = Path.GetDirectoryName(projectFile);

            if (_doc.Root == null)
                return new PageViewModel[0];

            _rootNs = _doc.Root.Name.Namespace;
            _pages = _doc.Root.Descendants(_rootNs + "Page")
                              .SelectMany(GetXamlPage)
                              .ToList();

            return _pages;
        }

        private IEnumerable<PageViewModel> GetXamlPage(XElement el)
        {
            var include = el.Attribute("Include");
            var includeValue = include?.Value;
                
            if (includeValue?.EndsWith(".xaml", StringComparison.InvariantCultureIgnoreCase) == true)
                return new[] { new PageViewModel(el, include, _projectDir) };
         
            return new PageViewModel[0];
        }

        public void SaveProject(bool saveBakFile)
        {
            foreach (var page in _pages.Where(p => p.NeedToConvert)) {
                page.Element.RemoveNodes();
                page.Element.Name = _rootNs + "None";
                page.IncludeAttribute.Value = Path.ChangeExtension(page.IncludeAttribute.Value, ".ammy");
            }

            foreach (var dependentUpon in _doc.Root.Descendants(_rootNs + "DependentUpon")) {
                var nodes = dependentUpon.Nodes().ToList();
                if (nodes.Count == 1 && nodes[0].NodeType == XmlNodeType.Text) {
                    var value = dependentUpon.Value;
                    var pageExists = _pages.FirstOrDefault(p => p.NeedToConvert && p.Filename == value) != null;

                    if (pageExists && Path.GetExtension(value) == ".xaml")
                        dependentUpon.Value = Path.ChangeExtension(value, ".ammy");
                }
            }

            if (saveBakFile)
                File.Move(_projectFilePath, _projectFilePath + ".bak");

            _doc.Save(_projectFilePath);
        }
    }
}