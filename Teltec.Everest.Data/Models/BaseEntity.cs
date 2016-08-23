using Teltec.Everest.Data.DAO.NH;
using Teltec.Common;

namespace Teltec.Everest.Data.Models
{
	public abstract class BaseEntity<ID> : ObservableObject, IPersistentEntity, IAuditable //where ID : class
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
