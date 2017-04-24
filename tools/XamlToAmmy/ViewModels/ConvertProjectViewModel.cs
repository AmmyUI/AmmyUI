using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;
using PropertyChanged;
using ReactiveUI;

namespace XamlToAmmy.ViewModels
{
    [ImplementPropertyChanged]
    class ConvertProjectViewModel
    {
        public ReactiveList<PageViewModel> Pages { get; private set; }
        public bool ProjectLoaded { get; set; }
        public bool CopyToBak { get; set; }

        public ReactiveCommand<Unit, string> LoadCsproj { get; }
        public ReactiveCommand<PageViewModel, Unit> PreviewPage { get; }
        public ReactiveCommand<Unit, Unit> Convert { get; }

        private readonly FileConverter _fileConverter;
        private readonly ProjectConverter _projectConverter;
        private readonly SettingsViewModel _settings;
        private readonly MainWindowViewModel _mainWindow;
        private string[] _pageFilenames = new string[0];
        private readonly ConvertFileViewModel _convertFileVm;

        public ConvertProjectViewModel(FileConverter fileConverter, ProjectConverter projectConverter, IOpenFileDialog openFileDialog, ConvertFileViewModel convertFileVm, SettingsViewModel settings, MainWindowViewModel mainWindow)
        {
            _convertFileVm = convertFileVm;
            _fileConverter = fileConverter;
            _projectConverter = projectConverter;
            _settings = settings;
            _mainWindow = mainWindow;

            CopyToBak = true;

            LoadCsproj = ReactiveCommand.CreateFromObservable(() => openFileDialog.BrowseFile());
            LoadCsproj.SelectMany(projectFile => LoadProjectFile(projectFile).ToObservable())
                      .Subscribe();

            PreviewPage = ReactiveCommand.Create((PageViewModel page) => {
                var originalFileName = File.Exists(page.FilePath) 
                                       ? page.FilePath
                                       : File.Exists(page.FilePath + ".bak") 
                                         ? page.FilePath + ".bak"
                                         : null;

                if (originalFileName != null)
                    convertFileVm.Xaml = File.ReadAllText(originalFileName);
                else
                    convertFileVm.Xaml = "File doesn't exist on disk anymore";
                
                UpdatePageFilenames();

                convertFileVm.Ammy = page.Ammy;

                _mainWindow.IsConvertFileSelected = true;
            });

            Convert = ReactiveCommand.Create(ConvertProject);
        }

        private void ConvertProject()
        {
            UpdatePageFilenames();

            foreach (var page in Pages.Where(p => p.NeedToConvert)) {
                var ammyFilePath = Path.ChangeExtension(page.FilePath, ".ammy");

                if (CopyToBak)
                    File.Move(page.FilePath, page.FilePath + ".bak");

                File.WriteAllText(ammyFilePath, page.Ammy);
            }

            _projectConverter.SaveProject(CopyToBak);

            MessageBox.Show("Conversion finished!" + 
                            Environment.NewLine + 
                            Environment.NewLine + 
                            "Execute `install-package Ammy.<WPF/XamarinForms/...>` after opening converted project.", "Success");

            Process.Start(_projectConverter.ProjectDir);
        }

        public async Task LoadProjectFile(string projectFile)
        {
            var pages = _projectConverter.LoadProject(projectFile);
            Pages = new ReactiveList<PageViewModel>(pages);
            ProjectLoaded = true;

            foreach (var page in Pages) {
                var result = await ConvertPage(page);
                page.Ammy = result.Item1;

                var warnings = result.Item2;
                if (warnings.Count == 0)
                    page.ConversionStatus = "OK";
                else
                    page.ConversionStatus = string.Join(Environment.NewLine, warnings);
            }
        }

        private void UpdatePageFilenames()
        {
            _pageFilenames = Pages.Where(p => p.NeedToConvert)
                                  .Select(p => p.Filename.Replace('\\', '/'))
                                  .ToArray();
            _convertFileVm.PageFilenames = _pageFilenames;
        }

        private Task<Tuple<string, IReadOnlyList<string>>> ConvertPage(PageViewModel page)
        {
            return Task.Run(() => {
                var xaml = File.ReadAllText(page.FilePath);
                var ammy = _fileConverter.Convert(xaml, _pageFilenames, _settings.CollapsedNodeMaxSize, _settings.IndentSize, _settings.OpeningBraceOnNewLine);
                
                return Tuple.Create(ammy, _fileConverter.Warnings);
            });
        }
    }
}