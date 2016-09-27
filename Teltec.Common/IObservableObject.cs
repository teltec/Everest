/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

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
