using System;
using System.Collections.Generic;
using Teltec.Common.Extensions;

namespace Teltec.Backup.Data.Models
{
	public class BackupPlan : SchedulablePlan<BackupPlan>
	{
		public override Type GetVirtualType()
		{
			return this.GetType();
		}

		#region Sources

		private IList<BackupPlanSourceEntry> _SelectedSources = new List<BackupPlanSourceEntry>();
		public virtual IList<BackupPlanSourceEntry> SelectedSources
		{
			get { return _SelectedSources; }
			protected set { SetField(ref _SelectedSources, value); InvalidateCachedSelectedSourcesAsDelimitedString(); }
		}

		private string _CachedSelectedSourcesAsDelimitedString;
		public virtual string SelectedSourcesAsDelimitedString(string delimiter, int maxLength, string trail)
		{
			if (_CachedSelectedSourcesAsDelimitedString == null)
				_CachedSelectedSourcesAsDelimitedString = SelectedSources.AsDelimitedString(p => p.Path,
					"No selected sources", delimiter, maxLength, trail);
			return _CachedSelectedSourcesAsDelimitedString;
		}

		public virtual void InvalidateCachedSelectedSourcesAsDelimitedString()
		{
			_CachedSelectedSourcesAsDelimitedString = null;
		}

		#endregion

		#region Files

		private IList<BackupPlanFile> _Files = new List<BackupPlanFile>();
		public virtual IList<BackupPlanFile> Files
		{
			get { return _Files; }
			protected set { SetField(ref _Files, value); }
		}

		#endregion

		#region Backups

		private IList<Backup> _Backups = new List<Backup>();
		public virtual IList<Backup> Backups
		{
			get { return _Backups; }
			protected set { SetField(ref _Backups, value); }
		}

		#endregion

		#region Purging

		private BackupPlanPurgeOptions _PurgeOptions = new BackupPlanPurgeOptions();
		public virtual BackupPlanPurgeOptions PurgeOptions
		{
			get { return _PurgeOptions; }
			set { SetField(ref _PurgeOptions, value); }
		}

		#endregion
	}
}
