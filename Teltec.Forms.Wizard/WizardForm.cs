/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Teltec.Common;
using Teltec.Common.Controls;
using Teltec.Common.Extensions;

namespace Teltec.Forms.Wizard
{
	public partial class WizardForm : ObservableForm
	{
		public WizardForm()
		{
			InitializeComponent();

			// Setup data bindings
			btnPrevious.DataBindings.Add(new Binding("Enabled", this,
				this.GetPropertyName((WizardForm x) => x.PreviousEnabled)));
			btnNext.DataBindings.Add(new Binding("Enabled", this,
				this.GetPropertyName((WizardForm x) => x.NextEnabled)));
			btnNext.DataBindings.Add(new NegateBinding("Visible",
				this, this.GetPropertyName((WizardForm x) => x.IsLastForm)));
			btnFinish.DataBindings.Add(new Binding("Enabled", this,
				this.GetPropertyName((WizardForm x) => x.FinishEnabled)));
			btnFinish.DataBindings.Add(new Binding("Visible", this,
				this.GetPropertyName((WizardForm x) => x.IsLastForm)));
		}

		#region Validation

		[
		Bindable(true),
		Category("Validation"),
		DefaultValue(true),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)
		]
		protected internal bool _DoValidate = true;
		protected internal bool DoValidate
		{
			get { return _DoValidate; }
			set { _DoValidate = value; }
		}

		protected virtual bool IsValid()
		{
			return true;
		}

		protected virtual void ShowErrorMessage(string caption, string message)
		{
			MessageBox.Show(message, caption);
		}

		protected virtual void ShowErrorMessage(string message)
		{
			MessageBox.Show(message);
		}

		#endregion

		#region Custom properties

		[
		Bindable(true),
		Category("Data"),
		DefaultValue(null),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
		]
		protected object _Model;
		public virtual object Model
		{
			get { return _Model; }
			set
			{
				SetField(ref _Model, value);
				OnModelChanged(this, new ModelChangedEventArgs(_Model));
			}
		}

		[
		Bindable(true),
		Category("Misc"),
		DefaultValue(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)
		]
		protected bool _FinishEnabled;
		public bool FinishEnabled
		{
			get { return _FinishEnabled; }
			set { SetField(ref _FinishEnabled, value); }
		}

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

		public class ModelChangedEventArgs : EventArgs
		{
			private object _model;
			public object Model
			{
				get { return _model; }
			}

			public ModelChangedEventArgs(object model)
			{
				_model = model;
			}
		}

		public delegate void ModelChangedEventHandler(object sender, ModelChangedEventArgs e);
		public delegate void WizardActionEventHandler(object sender, EventArgs e);
		public delegate void WizardCancellableActionEventHandler(object sender, CancelEventArgs e);

		public event ModelChangedEventHandler ModelChangedEvent;
		public event WizardActionEventHandler FormClosedEvent;
		public event WizardActionEventHandler CancelEvent;
		public event WizardActionEventHandler PreviousEvent;
		public event WizardCancellableActionEventHandler NextEvent;
		public event WizardCancellableActionEventHandler FinishEvent;
		public event WizardCancellableActionEventHandler BeforeNextOrFinishEvent;

		protected virtual void OnModelChanged(object sender, ModelChangedEventArgs e)
		{
			if (ModelChangedEvent != null)
				ModelChangedEvent(this, e);
		}

		protected virtual void OnFormClosed(object sender, EventArgs e)
		{
			if (FormClosedEvent != null)
				FormClosedEvent(this, e);
		}

		protected virtual void OnCancel(object sender, EventArgs e)
		{
			if (CancelEvent != null)
				CancelEvent(this, e);
		}

		protected virtual void OnPrevious(object sender, EventArgs e)
		{
			if (PreviousEvent != null)
				PreviousEvent(this, e);
		}

		protected virtual void OnNext(object sender, CancelEventArgs e)
		{
			if (!e.Cancel && NextEvent != null)
				NextEvent(this, e);
		}

		protected virtual void OnFinish(object sender, CancelEventArgs e)
		{
			if (!e.Cancel && FinishEvent != null)
				FinishEvent(this, e);
		}

		protected virtual void OnBeforeNextOrFinish(object sender, CancelEventArgs e)
		{
			if (!e.Cancel && BeforeNextOrFinishEvent != null)
				BeforeNextOrFinishEvent(this, e);
		}

		#endregion

		#region Form events

		private void WizardForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			OnFormClosed(sender, e);
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			OnCancel(sender, e);
		}

		private void btnPrevious_Click(object sender, EventArgs e)
		{
			OnPrevious(sender, e);
		}

		private void btnFinish_Click(object sender, EventArgs e)
		{
			CancelEventArgs args = new CancelEventArgs();
			OnBeforeNextOrFinish(sender, args);
			if (!args.Cancel)
				OnFinish(sender, args);
		}

		private void btnNext_Click(object sender, EventArgs e)
		{
			CancelEventArgs args = new CancelEventArgs();
			OnBeforeNextOrFinish(sender, args);
			if (!args.Cancel)
				OnNext(sender, args);
		}

		#endregion
	}
}
