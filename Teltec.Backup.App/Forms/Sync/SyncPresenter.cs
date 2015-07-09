using NLog;
using System;
using Teltec.Backup.App.Forms.Sync;
using Teltec.Backup.Data.DAO;
using Teltec.Forms.Wizard;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.App.Forms.BackupPlan
{
	sealed class SyncPresenter : WizardPresenter
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly SynchronizationRepository _dao = new SynchronizationRepository();

		public SyncPresenter()
			: this(new Models.Synchronization())
		{
			IsEditingModel = false;

			//Models.Synchronization sync = Model as Models.Synchronization;
			//sync.aaa = bbb;
		}

		public SyncPresenter(Models.Synchronization sync)
		{
			IsEditingModel = true;
			Model = sync;

			WizardFormOptions options = new WizardFormOptions { DoValidate = true };
			RegisterFormClass(typeof(SyncSelectAccountForm), options);
			RegisterFormClass(typeof(SyncProgressForm), options);
			//RegisterFormClass(typeof(SyncFinishedForm), options);
		}

		public override void OnCancel()
		{
			base.OnCancel();

			Models.Synchronization sync = Model as Models.Synchronization;
			if (sync.Id.HasValue)
				_dao.Refresh(sync);
		}

		public override void OnFinish()
		{
			base.OnFinish();

			Models.Synchronization sync = Model as Models.Synchronization;

			Console.WriteLine("Status = {0}", sync.Status);
			Console.WriteLine("StorageAccount = {0}", sync.StorageAccount.DisplayName);
			Console.WriteLine("StorageAccountType = {0}", sync.StorageAccountType.ToString());
			foreach (Models.BackupedFile entry in sync.Files)
				Console.WriteLine("SyncedFile => #{0}, {1}, {2}", entry.Id, entry.File.Path, entry.Version);

			//try
			//{
			//	if (IsEditingModel)
			//	{
			//		_dao.Update(sync);
			//	}
			//	else
			//	{
			//		_dao.Insert(sync);
			//	}
			//}
			//catch (Exception ex)
			//{
			//	MessageBox.Show(ex.Message, "Error");
			//}
		}

		/*
		public static string NormalizePlanName(string planName)
		{
			string normalized = planName.Replace(' ', '_').RemoveDiacritics();

			Regex r = new Regex("^[a-zA-Z0-9_-]+$");
			if (r.IsMatch(normalized))
				return normalized;

			throw new ApplicationException("The plan name still contains characters that may cause problems");
		}
		*/
	}
}
