using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Teltec.Backup.App.Controls
{
	public class DriveItemsBindingSource : BindingSource
	{
		private DriveItemsEnumerable Data = new DriveItemsEnumerable();

		public DriveItemsBindingSource() : base()
		{
			AllowNew = false;
			//BindingComplete += this_BindingComplete;
		}

		public DriveItemsBindingSource(IContainer container)
			: base(container)
		{
			AllowNew = false;
			//BindingComplete += this_BindingComplete;
		}

		//void this_BindingComplete(object sender, BindingCompleteEventArgs e)
		//{
		//	MessageBox.Show("BindingComplete");
		//	if (e.BindingCompleteState != BindingCompleteState.Success)
		//	{
		//		string message = string.Format("{0} error during binding: {0}.",
		//			typeof(DriveItemsBindingSource), e.ErrorText);
		//		MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
		//		Environment.Exit(1);
		//	}
		//}

		public void BindData()
		{
			DataSource = Data;
		}

		public DriveItemsBindingSource(object dataSource, string dataMember)
			: base(dataSource, dataMember)
		{
			string message = string.Format("Does not support changing the dataSource of a {0} object",
				typeof(DriveItemsBindingSource).Name);
			throw new NotSupportedException(message);
		}

		[
		Browsable(true),
		Category("Custom"),
		Description("Specifies whether to show Removable drives (default is true)"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
		]
		public bool HideRemovableDrives
		{
			get { return Data.ExcludeRemovableDrives; }
			set { Data.ExcludeRemovableDrives = value; }
		}

		[
		Browsable(true),
		Category("Custom"),
		Description("Specifies whether to show Fixed drives (default is true)"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
		]
		public bool HideFixedDrives
		{
			get { return Data.ExcludeFixedDrives; }
			set { Data.ExcludeFixedDrives = value; }
		}

		[
		Browsable(true),
		Category("Custom"),
		Description("Specifies whether to show CDRom drives (default is true)"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
		]
		public bool HideCDRomDrives
		{
			get { return Data.ExcludeCDRomDrives; }
			set { Data.ExcludeCDRomDrives = value; }
		}

		[
		Browsable(true),
		Category("Custom"),
		Description("Specifies whether to show Network drives (default is true)"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
		]
		public bool HideNetworkDrives
		{
			get { return Data.ExcludeNetworkDrives; }
			set { Data.ExcludeNetworkDrives = value; }
		}
	}
}
