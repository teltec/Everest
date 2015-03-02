﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teltec.Common.Extensions;

namespace Teltec.Backup.App.Forms.BackupPlan
{
	public partial class BackupPlanGiveNameForm : Teltec.Forms.Wizard.WizardForm
	{
		private Models.BackupPlan Plan = new Models.BackupPlan();

		public BackupPlanGiveNameForm()
		{
			InitializeComponent();
			this.ModelChangedEvent += (sender, args) => {
				this.Plan = args.Model as Models.BackupPlan;
				
				// Setup data bindings
				textBox1.DataBindings.Clear();
				textBox1.DataBindings.Add(new Binding("Text", this.Plan,
					this.GetPropertyName((Models.BackupPlan x) => x.Name)));
			};
		}

		protected override bool IsValid()
		{
			bool emptyName = String.IsNullOrEmpty(this.Plan.Name);
			return !emptyName;
		}

		protected override void OnBeforeNextOrFinish(object sender, CancelEventArgs e)
		{
			if (DoValidate && !IsValid())
			{
				e.Cancel = true;
				this.ShowErrorMessage("Please, inform a name.");
			}
			base.OnBeforeNextOrFinish(sender, e);
		}
	}
}
