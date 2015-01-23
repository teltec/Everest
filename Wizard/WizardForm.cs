using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teltec.Common;
using Teltec.Common.Extensions;
using Teltec.Common.Forms;

namespace Teltec.Forms.Wizard
{
	public partial class WizardForm : ObservableForm
	{
		public WizardForm()
		{
			InitializeComponent();

			// Setup data bindings
			btnPrevious.DataBindings.Add(new Binding("Enabled", this, this.GetPropertyName((WizardForm x) => x.PreviousEnabled), false, DataSourceUpdateMode.OnPropertyChanged));
			btnNext.DataBindings.Add(new Binding("Enabled", this, this.GetPropertyName((WizardForm x) => x.NextEnabled), false, DataSourceUpdateMode.OnPropertyChanged));
			btnNext.DataBindings.Add(new NegateBinding("Visible", this, this.GetPropertyName((WizardForm x) => x.IsLastForm), false, DataSourceUpdateMode.OnPropertyChanged));
			btnFinish.DataBindings.Add(new Binding("Visible", this, this.GetPropertyName((WizardForm x) => x.IsLastForm), false, DataSourceUpdateMode.OnPropertyChanged));
		}

		#region Custom properties

		[
		Bindable(true),
		Category("Misc"),
		DefaultValue(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)
		]
		protected bool _NextEnabled;
		public bool NextEnabled
		{
			get { return _NextEnabled; }
			set { SetField(ref _NextEnabled, value); }
		}

		[
		Bindable(true),
		Category("Misc"),
		DefaultValue(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)
		]
		protected bool _PreviousEnabled;
		public bool PreviousEnabled
		{
			get { return _PreviousEnabled; }
			set { SetField(ref _PreviousEnabled, value); }
		}

		[
		Bindable(true),
		Category("Misc"),
		DefaultValue(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)
		]
		protected bool _IsLastForm;
		public bool IsLastForm
		{
			get { return _IsLastForm; }
			set { SetField(ref _IsLastForm, value); }
		}

		#endregion

		#region Custom events

		public delegate void CancelEventHandler(WizardForm sender, EventArgs e);
		public delegate void FinishEventHandler(WizardForm sender, EventArgs e);
		public delegate void NextEventHandler(WizardForm sender, EventArgs e);
		public delegate void PreviousEventHandler(WizardForm sender, EventArgs e);

		public event CancelEventHandler CancelEvent;
		public event FinishEventHandler FinishEvent;
		public event NextEventHandler NextEvent;
		public event PreviousEventHandler PreviousEvent;

		protected virtual void OnCancel(object sender, EventArgs e)
		{
			if (CancelEvent != null)
				CancelEvent(this, e);
		}

		protected virtual void OnFinish(object sender, EventArgs e)
		{
			if (FinishEvent != null)
				FinishEvent(this, e);
		}

		protected virtual void OnNext(object sender, EventArgs e)
		{
			if (NextEvent != null)
				NextEvent(this, e);
		}

		protected virtual void OnPrevious(object sender, EventArgs e)
		{
			if (PreviousEvent != null)
				PreviousEvent(this, e);
		}

		#endregion

		#region Form events

		private void btnCancel_Click(object sender, EventArgs e)
		{
			OnCancel(sender, e);
		}

		private void btnFinish_Click(object sender, EventArgs e)
		{
			OnFinish(sender, e);
		}

		private void btnPrevious_Click(object sender, EventArgs e)
		{
			OnPrevious(sender, e);
		}

		private void btnNext_Click(object sender, EventArgs e)
		{
			OnNext(sender, e);
		}

		#endregion

	}
}
