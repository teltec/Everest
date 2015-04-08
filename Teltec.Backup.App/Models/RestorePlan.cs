using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teltec.Common.Extensions;

namespace Teltec.Backup.App.Models
{
	public class RestorePlan : BaseEntity<Int32?>
	{
		private Int32? _Id;
		public virtual Int32? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

				#region Name

		public const int NameMaxLen = 128;
		private String _Name;
		public virtual String Name
		{
			get { return _Name; }
			set { SetField(ref _Name, value); }
		}

		#endregion

		#region Backup Plan

		private BackupPlan _BackupPlan;
		public virtual BackupPlan BackupPlan
		{
			get { return _BackupPlan; }
			set { SetField(ref _BackupPlan, value); }
		}

		#endregion

		#region Sources

		private IList<RestorePlanSourceEntry> _SelectedSources = new List<RestorePlanSourceEntry>();
		public virtual IList<RestorePlanSourceEntry> SelectedSources
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

		private void InvalidateCachedSelectedSourcesAsDelimitedString()
		{
			_CachedSelectedSourcesAsDelimitedString = null;
		}

		#endregion

		#region Files

		private IList<RestorePlanFile> _Files = new List<RestorePlanFile>();
		public virtual IList<RestorePlanFile> Files
		{
			get { return _Files; }
			protected set { SetField(ref _Files, value); }
		}

		#endregion

		#region Restores

		private IList<Restore> _Restores = new List<Restore>();
		public virtual IList<Restore> Restores
		{
			get { return _Restores; }
			protected set { SetField(ref _Restores, value); }
		}

		#endregion

		#region Schedule

		public enum EScheduleType
		{
			RunManually = 0,
		}

		private EScheduleType _ScheduleType;
		public virtual EScheduleType ScheduleType
		{
			get { return _ScheduleType; }
			set { SetField(ref _ScheduleType, value); }
		}

		public virtual bool IsRunManually
		{
			get { return ScheduleType == EScheduleType.RunManually; }
		}

		#endregion

		private DateTime? _LastRunAt;
		public virtual DateTime? LastRunAt
		{
			get { return _LastRunAt; }
			set { SetField(ref _LastRunAt, value); }
		}

		private DateTime? _LastSuccessfulRunAt;
		public virtual DateTime? LastSuccessfulRunAt
		{
			get { return _LastSuccessfulRunAt; }
			set { SetField(ref _LastSuccessfulRunAt, value); }
		}
	}
}
