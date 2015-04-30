using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Teltec.Common
{
	public class ObservableUserControl : UserControl, IObservableObject
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
