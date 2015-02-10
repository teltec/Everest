using System;
using NHibernate.Type;
using System.Collections;
using NHibernate;

namespace Teltec.Backup.DAO
{

	public interface IAuditable
	{
	}

	// REFERENCE: http://nhibernate.info/doc/nh/en/index.html
	[Serializable]
	public class NHibernateAuditInterceptor : EmptyInterceptor
	{
		private int updates;
		private int creates;
		private int loads;

		public override void OnDelete(object entity,
							 object id,
							 object[] state,
							 string[] propertyNames,
							 IType[] types)
		{
			// do nothing
		}

		public override bool OnFlushDirty(object entity,
									object id,
									object[] currentState,
									object[] previousState,
									string[] propertyNames,
									IType[] types)
		{
			if (entity is IAuditable)
			{
				updates++;
				//for (int i = 0; i < propertyNames.Length; i++)
				//{
				//	if ("LastUpdateTimestamp" == propertyNames[i])
				//	{
				//		currentState[i] = DateTime.Now;
				//		return true;
				//	}
				//}
			}
			return false;
		}

		public override bool OnLoad(object entity,
							  object id,
							  object[] state,
							  string[] propertyNames,
							  IType[] types)
		{
			if (entity is IAuditable)
			{
				loads++;
			}
			return false;
		}

		public override bool OnSave(object entity,
							  object id,
							  object[] state,
							  string[] propertyNames,
							  IType[] types)
		{
			if (entity is IAuditable)
			{
				creates++;
				//for (int i = 0; i < propertyNames.Length; i++)
				//{
				//	if ("CreateTimestamp" == propertyNames[i])
				//	{
				//		state[i] = DateTime.Now;
				//		return true;
				//	}
				//}
			}
			return false;
		}

		public override void AfterTransactionCompletion(ITransaction tx)
		{
			if (tx.WasCommitted)
			{
				System.Console.WriteLine("Creations: " + creates + ", Updates: " + updates, "Loads: " + loads);
			}
			updates = 0;
			creates = 0;
			loads = 0;
		}
	}
}
