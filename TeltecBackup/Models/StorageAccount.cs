using NHibernate.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teltec.Common;

namespace Teltec.Backup.Models
{
	public abstract class StorageAccount : BaseEntity<int?>
    {
		private int? _Id;
		public virtual int? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		public abstract EStorageAccountType Type
		{
			get;
		}

		private String _DisplayName;
		public virtual String DisplayName
		{
			get { return _DisplayName; }
			set { SetField(ref _DisplayName, value); }
		}

		//IList<BackupPlan> BackupPlans { get; set; }
    }
}
