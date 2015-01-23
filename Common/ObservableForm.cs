﻿using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Teltec.Common
{
    public class ObservableForm : Form, IObservableObject
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected ObservableObject _ObservableObj = new ObservableObject();

        public void NotifyPropertyChanged(string propertyName)
        {
            _ObservableObj.NotifyPropertyChanged(propertyName);
        }

        public void NotifyPropertyChanged<TProperty>(Expression<Func<TProperty>> property)
        {
            _ObservableObj.NotifyPropertyChanged(property);
        }

        public bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            return _ObservableObj.SetField(ref field, value, propertyName);
        }
    }
}
