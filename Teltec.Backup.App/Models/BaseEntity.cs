using Teltec.Backup.App.DAO.NH;
using Teltec.Common;

namespace Teltec.Backup.App.Models
{
	public abstract class BaseEntity<ID> : ObservableObject, IPersistentEntity //where ID : class
	{
		private bool _saved = false;

		public virtual void OnSave()
		{
			_saved = true;
		}

		public virtual void OnLoad()
		{
			_saved = true;
		}

		public virtual bool IsSaved
		{
			get { return _saved; }
		}
	}
}
