using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teltec.Common;

namespace Teltec.Backup.Models
{
	public class BackupTask : ObservableObject
	{
		private BackupPlan _Plan;
		public virtual BackupPlan Plan
		{
			get { return _Plan; }
			set { SetField(ref _Plan, value); }
		}
	}
}
