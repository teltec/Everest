using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Teltec.Common
{
    interface IObservableObject : INotifyPropertyChanged
    {
        // NotifyPropertyChanged will raise the PropertyChanged event passing the
        // source property that is being updated.
        void NotifyPropertyChanged(string propertyName);

        void NotifyPropertyChanged<TProperty>(Expression<Func<TProperty>> property);

        bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null);
    }
}
