using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teltec.Common;

namespace Teltec.Backup.Models
{
	public class Backup : BaseEntity<Int32>
	{
		private DateTime _StartedAt;
		public DateTime StartedAt
		{
			get { return _StartedAt; }
			set { SetField(ref _StartedAt, value); }
		}

		private DateTime _FinishedAt;
		public DateTime FinishedAt
		{
			get { return _FinishedAt; }
			set { SetField(ref _FinishedAt, value); }
		}

		#region Tasks

		private IList<BackupTask> _Tasks = new List<BackupTask>();

		public virtual IList<BackupTask> Tasks
		{
			get { return _Tasks; }
		}

		public void ClearTasks()
		{
			_Tasks.Clear();
		}

		public void AddTask(BackupTask task)
		{
			_Tasks.Add(task);
		}

		public int TaskCount
		{
			get { return _Tasks.Count; }
		}

		public void Start()
		{
			foreach (var task in _Tasks)
				StartTask(task);
		}

		public BackupStatus StartTask(BackupTask task)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
