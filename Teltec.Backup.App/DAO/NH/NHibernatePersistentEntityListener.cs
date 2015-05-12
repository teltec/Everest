using NHibernate.Event;

namespace Teltec.Backup.App.DAO.NH
{
	// REFERENCE: http://nhibernate.info/doc/nh/en/index.html
	public class NHibernatePersistentEntityListener : IPostLoadEventListener, ISaveOrUpdateEventListener
	{
		void IPostLoadEventListener.OnPostLoad(PostLoadEvent @event)
		{
			if (@event.Entity is IPersistentEntity)
			{
				(@event.Entity as IPersistentEntity).OnLoad();
			}
		}

		void ISaveOrUpdateEventListener.OnSaveOrUpdate(SaveOrUpdateEvent @event)
		{
			if (@event.Entity is IPersistentEntity)
			{
				(@event.Entity as IPersistentEntity).OnSave();
			}
		}
	}
}
