using NHibernate;
using NHibernate.Type;
using NLog;
using System;

namespace Teltec.Backup.DAO.NHibernate
{
	// REFERENCE: http://nhibernate.info/doc/nh/en/index.html
	[Serializable]
	public class NHibernateAuditInterceptor : EmptyInterceptor
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private int updates;
		private int creates;
		private int loads;

		/// <summary>
		/// Called when a transient entity is passed to <c>SaveOrUpdate</c>.
		/// </summary>
		/// <remarks>
		///	The return value determines if the object is saved
		///	<list>
		///		<item><see langword="true" /> - the entity is passed to <c>Save()</c>, resulting in an <c>INSERT</c></item>
		///		<item><see langword="false" /> - the entity is passed to <c>Update()</c>, resulting in an <c>UPDATE</c></item>
		///		<item><see langword="null" /> - Hibernate uses the <c>unsaved-value</c> mapping to determine if the object is unsaved</item>
		///	</list>
		/// </remarks>
		/// <param name="entity">A transient entity</param>
		/// <returns>Boolean or <see langword="null" /> to choose default behaviour</returns
		//public override bool? IsTransient(object entity)
		//{
		//	if (entity is IPersistentEntity)
		//	{
		//		return !(entity as IPersistentEntity).IsSaved;
		//	}
		//	return null;
		//}

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
			if (tx == null)
				return;

			if (tx.WasCommitted)
			{
				logger.Debug("Creations: {0}, Updates: {1}, Loads: {2}", creates, updates, loads);
			}
			updates = 0;
			creates = 0;
			loads = 0;
		}
	}
}
