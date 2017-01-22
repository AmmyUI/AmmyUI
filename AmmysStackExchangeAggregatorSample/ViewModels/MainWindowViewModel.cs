using PropertyChanged;
using AmmySEA.StackExchangeApi;
using System;

namespace AmmySEA.ViewModels
{
    [ImplementPropertyChanged]
    class MainWindowViewModel
    {
        public OptionsViewModel Options { get; private set; }
        public QuestionBlockListViewModel QuestionBlockList { get; set; }

        private Api _api = new Api();

        public MainWindowViewModel()
        {
            Options = new OptionsViewModel(_api);
            QuestionBlockList = new QuestionBlockListViewModel(_api, Options);
        }
    }
}
