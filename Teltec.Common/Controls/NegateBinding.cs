using System.Windows.Forms;

namespace Teltec.Common.Controls
{
	public class NegateBinding
	{
		string propertyName;
		object dataSource;
		string dataMember;
		bool formattingEnabled;
		DataSourceUpdateMode dataSourceUpdateMode;

		public NegateBinding(string propertyName, object dataSource, string dataMember)
			: this(propertyName, dataSource, dataMember, false, DataSourceUpdateMode.OnPropertyChanged)
		{
		}

		public NegateBinding(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode)
		{
			this.propertyName = propertyName;
			this.dataSource = dataSource;
			this.dataMember = dataMember;
			this.formattingEnabled = formattingEnabled;
			this.dataSourceUpdateMode = dataSourceUpdateMode;
		}

		public static implicit operator Binding(NegateBinding eb)
		{
			var binding = new Binding(eb.propertyName, eb.dataSource, eb.dataMember, eb.formattingEnabled, eb.dataSourceUpdateMode);
			binding.Parse += new ConvertEventHandler(negate);
			binding.Format += new ConvertEventHandler(negate);
			return binding;
		}

		static void negate(object sender, ConvertEventArgs e)
		{
			e.Value = !((bool)e.Value);
		}
	}
}
