using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DataGridSample
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _newGuestFirstName;
        private string _newGuestLastName;

        public ObservableCollection<Guest> Guests { get; } = new ObservableCollection<Guest>();
        public ICommand AddGuest { get; }

        public string NewGuestFirstName {
            get { return _newGuestFirstName; }
            set {
                if (value == _newGuestFirstName) return;
                _newGuestFirstName = value;
                OnPropertyChanged();
            }
        }

        public string NewGuestLastName {
            get { return _newGuestLastName; }
            set {
                if (value == _newGuestLastName) return;
                _newGuestLastName = value;
                OnPropertyChanged();
            }
        }

        public bool NewGuestIsMale { get; set; }

        public MainWindowViewModel()
        {
            AddGuest = new AddGuestCommand(this);
            NewGuestIsMale = true;

            Guests.Add(new Guest("Walter", "White", Gender.Male));
            Guests.Add(new Guest("Daenerys", "Targaryen", Gender.Female));
            Guests.Add(new Guest("James", "McNulty", Gender.Male));
            Guests.Add(new Guest("Rick", "Sanchez", Gender.Male));
            Guests.Add(new Guest("Dolores", "Abernathy", Gender.Female));
            Guests.Add(new Guest("Rust", "Cohle", Gender.Male));
            Guests.Add(new Guest("Joyce", "Byers", Gender.Female));
            Guests.Add(new Guest("Lorne", "Malvo", Gender.Male));
            Guests.Add(new Guest("Francis", "Underwood", Gender.Male));
            //Guests.Add(new Guest("Dee", "Reynolds", Gender.Female));
            //Guests.Add(new Guest("Pam", "Beesly", Gender.Female));
            //Guests.Add(new Guest("Jimmy", "McGill", Gender.Male));
        }


        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}