/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Teltec.Common
{
	public class ObservableForm : Form, IObservableObject
	{
#pragma warning disable 0067
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067
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
