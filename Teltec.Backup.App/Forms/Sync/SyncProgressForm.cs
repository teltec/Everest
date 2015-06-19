using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.App.Forms.Sync
{
	public partial class SyncProgressForm : Teltec.Forms.Wizard.WizardForm
	{
		private Models.Synchronization Sync = new Models.Synchronization();

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();
			}
			base.Dispose(disposing);
		}

		public SyncProgressForm()
		{
			InitializeComponent();

			this.PreviousEnabled = false;
			this.NextEnabled = false;

			this.ModelChangedEvent += (sender, args) =>
			{
				this.Sync = args.Model as Models.Synchronization;

				switch (this.Sync.StorageAccountType)
				{
					case Models.EStorageAccountType.AmazonS3:
						break;
					case Models.EStorageAccountType.FileSystem:
						break;
				}
			};
		}
	}
}
