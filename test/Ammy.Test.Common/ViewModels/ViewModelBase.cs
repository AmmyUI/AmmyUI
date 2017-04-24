using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Ammy.Test.Common.Annotations;

namespace AmmyTest.Common.ViewModels
{
    public abstract class ViewModelBase<TViewModel> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged<TValue>(Expression<Func<TViewModel, TValue>> propertySelector)
        {
            if (PropertyChanged == null)
                return;

            var memberExpression = propertySelector.Body as MemberExpression;
            if (memberExpression == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }
    }
}