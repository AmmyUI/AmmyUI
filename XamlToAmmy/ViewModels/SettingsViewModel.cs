using PropertyChanged;

namespace XamlToAmmy.ViewModels
{
    [ImplementPropertyChanged]
    public class SettingsViewModel
    {
        public int CollapsedNodeMaxSize { get; set; }
        public int IndentSize { get; set; }
        public bool OpeningBraceOnNewLine { get; set; }

        public SettingsViewModel()
        {
            CollapsedNodeMaxSize = 2;
            IndentSize = 2;
            OpeningBraceOnNewLine = false;
        }
    }
}