using System;
using System.Windows;
using System.Windows.Input;

namespace AmmyTest.Common.ViewModels
{
    public class TestViewModel : ViewModelBase<TestViewModel>
    {
        private string _firstName;

        public string FirstName
        {
            get { return _firstName; }
            set {
                _firstName = value;
                OnPropertyChanged(vm => vm.FirstName);
            }
        }

        private string _lastName;

        public string LastName
        {
            get { return _lastName; }
            set {
                _lastName = value; 
                OnPropertyChanged(vm => vm.LastName);
            }
        }

        public static void OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        public static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        public static object ExceptionFilter(object bindingExpression, Exception exception)
        {
            throw new System.NotImplementedException();
        }

        public static void MouseRightButtonDown(object sender, MouseButtonEventArgs e) 
        { 
        }

        public static void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e) 
        { 
            e.CanExecute = true; 
        }

        public static void HelpExecuted(object sender, ExecutedRoutedEventArgs e) 
        { 
            System.Diagnostics.Process.Start("http://www.google.com"); 
        }
    }
}