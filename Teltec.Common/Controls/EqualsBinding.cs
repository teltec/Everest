using System;
using System.Windows.Forms;

namespace Teltec.Common.Controls
{
	public class EqualsBinding<T> where T : struct, IConvertible
	{
		string propertyName;
		object dataSource;
		string dataMember;
		bool formattingEnabled;
		DataSourceUpdateMode dataSourceUpdateMode;
		T expectedValue;

		public EqualsBinding(string propertyName, object dataSource, string dataMember, T expectedValue)
			: this(propertyName, dataSource, dataMember, false, DataSourceUpdateMode.OnPropertyChanged)
		{
			this.expectedValue = expectedValue;
		}

		protected EqualsBinding(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode)
		{
			this.propertyName = propertyName;
			this.dataSource = dataSource;
			this.dataMember = dataMember;
			this.formattingEnabled = formattingEnabled;
			this.dataSourceUpdateMode = dataSourceUpdateMode;
		}

		public static implicit operator Binding(EqualsBinding<T> eb)
		{
			var binding = new BinaryBinding(eb.propertyName, eb.dataSource, eb.dataMember, eb.formattingEnabled, eb.dataSourceUpdateMode, eb.expectedValue);
			binding.Parse += new ConvertEventHandler(equals);
			binding.Format += new ConvertEventHandler(equals);
			return binding;
		}

		static void equals(object sender, ConvertEventArgs e)
		{
			BinaryBinding obj = sender as BinaryBinding;
			e.Value = obj.IsEqual(e.Value);
		}

		private class BinaryBinding : Binding
		{
			T ExpectedValue;

			public BinaryBinding(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode, T expectedValue)
				: base(propertyName, dataSource, dataMember, formattingEnabled, dataSourceUpdateMode)
			{
				this.ExpectedValue = expectedValue;
			}

			public bool IsEqual(object value)
			{
				return value.Equals(this.ExpectedValue);
			}
		}
	}
}
