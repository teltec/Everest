using NHibernate.Event;

namespace Teltec.Everest.Data.DAO.NH
{
	// REFERENCE: http://nhibernate.info/doc/nh/en/index.html
	public class NHibernateLoadListener : ILoadEventListener
	{
		// this is the single method defined by the LoadEventListener interface
		public void OnLoad(LoadEvent theEvent, LoadType loadType)
		{
			//if (!MySecurity.IsAuthorized(theEvent.EntityClassName, theEvent.EntityId))
			//{
			//	throw new MySecurityException("Unauthorized access");
			//}
		}
	}
}
