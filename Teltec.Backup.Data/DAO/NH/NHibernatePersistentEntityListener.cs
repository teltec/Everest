using NHibernate.Event;

namespace Teltec.Backup.Data.DAO.NH
{
	// REFERENCE: http://nhibernate.info/doc/nh/en/index.html
	public class NHibernatePersistentEntityListener : IPostLoadEventListener, ISaveOrUpdateEventListener
	{
		#region IPostLoadEventListener

		public void OnPostLoad(PostLoadEvent @event)
		{
			if (@event.Entity is IPersistentEntity)
			{
				(@event.Entity as IPersistentEntity).OnLoad();
			}
		}

		#endregion

		#region ISaveOrUpdateEventListener

		public void OnSaveOrUpdate(SaveOrUpdateEvent @event)
		{
			if (@event.Entity is IPersistentEntity)
			{
				(@event.Entity as IPersistentEntity).OnSave();
			}
		}

		#endregion
	}
}
