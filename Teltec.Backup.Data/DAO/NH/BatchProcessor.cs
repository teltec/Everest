using NHibernate;

namespace Teltec.Backup.Data.DAO.NH
{
	public class BatchProcessor
	{
		private short BatchCounter = 0;

		public bool ProcessBatch(ISession session, bool forceFlush = false)
		{
			bool didFlush = false;

			++BatchCounter;

			if (BatchCounter % NHibernateHelper.BatchSize == 0 || forceFlush)
			{
				// Flush a batch of operations and release memory.
				if (session != null)
				{
					session.Flush();
					session.Clear();
					didFlush = true;
				}

				BatchCounter = 0;
			}

			return didFlush;
		}
	}
}
