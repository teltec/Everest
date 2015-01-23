using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Teltec.Backup.Models;

namespace Teltec.Backup.Forms.BackupPlan
{
    public partial class BackupPlanAccountSelectionForm : Teltec.Forms.Wizard.WizardForm
    {
        public AmazonS3Account _account;

		public enum AccountType {
			AmazonS3,
			FileSystem
		};

        public BackupPlanAccountSelectionForm()
        {
            InitializeComponent();
        }

		private bool IsValid()
		{
			bool didSelectAccount = rdbtAmazonS3.Checked || rdbtnFileSystem.Checked;
			bool somethingElse = true;

			return didSelectAccount && somethingElse;
		}

		protected override void OnCancel(object sender, EventArgs e)
		{
			Console.WriteLine("OnCancel");
			base.OnCancel(sender, e);
		}

		protected override void OnFinish(object sender, EventArgs e)
		{
			Console.WriteLine("OnFinish");
			base.OnFinish(sender, e);
		}

		protected override void OnNext(object sender, EventArgs e)
		{
			Console.WriteLine("OnNext");
			if (!IsValid())
			{
				MessageBox.Show("Please, select an account");
				return;
			}
			base.OnNext(sender, e);
		}

		protected override void OnPrevious(object sender, EventArgs e)
		{
			Console.WriteLine("OnPrevious");
			base.OnPrevious(sender, e);
		}

    }
}