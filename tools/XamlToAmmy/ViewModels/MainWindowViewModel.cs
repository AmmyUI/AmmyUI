using PropertyChanged;

namespace XamlToAmmy.ViewModels
{
    [ImplementPropertyChanged]
    class MainWindowViewModel
    {
        public ConvertFileViewModel File { get; set; }
        public ConvertProjectViewModel Project { get; set; }
        public SettingsViewModel Settings { get; set; }
        public bool IsConvertFileSelected { get; set; }

        public MainWindowViewModel(IOpenFileDialog openFileDialog)
        {
            var fileConverter = new FileConverter();
            var projectConverter = new ProjectConverter();

            Settings = new SettingsViewModel();
            File = new ConvertFileViewModel(fileConverter, Settings);
            Project = new ConvertProjectViewModel(fileConverter, projectConverter, openFileDialog, File, Settings, this);
        }
    }
}
