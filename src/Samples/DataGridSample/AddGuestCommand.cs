using System;
using System.Windows.Input;

namespace DataGridSample
{
    public class AddGuestCommand : ICommand
    {
        private readonly MainWindowViewModel _vm;
        private bool? _previousCanExecute;
        public AddGuestCommand(MainWindowViewModel vm)
        {
            _vm = vm;
            _vm.PropertyChanged += (sender, args) => {
                var canExecute = CanExecute(null);

                if (canExecute != _previousCanExecute) {
                    CanExecuteChanged?.Invoke(this, new EventArgs());
                    _previousCanExecute = canExecute;
                }
            };
        }

        public bool CanExecute(object parameter)
        {
            return !string.IsNullOrEmpty(_vm.NewGuestFirstName) &&
                   !string.IsNullOrEmpty(_vm.NewGuestLastName);
        }

        public void Execute(object parameter)
        {
            var gender = _vm.NewGuestIsMale ? Gender.Male : Gender.Female;
            _vm.Guests.Add(new Guest(_vm.NewGuestFirstName, _vm.NewGuestLastName, gender));
            _vm.NewGuestFirstName = "";
            _vm.NewGuestLastName = "";
        }

        public event EventHandler CanExecuteChanged;
    }
}