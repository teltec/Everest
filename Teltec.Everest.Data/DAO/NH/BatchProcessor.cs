/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NHibernate;
using NUnit.Framework;

namespace Teltec.Everest.Data.DAO.NH
{
	public class BatchProcessor
	{
		private short BatchCounter = 0;
		private readonly short BatchSize = NHibernateHelper.BatchSize;

		public BatchProcessor()
		{
		}

		public BatchProcessor(short batchSize)
		{
			Assert.GreaterOrEqual(batchSize, 1);
			BatchSize = batchSize;
		}

		public bool ProcessBatch(ISession session, bool forceFlush = false)
		{
			bool didFlush = false;

			++BatchCounter;

			if (BatchCounter % BatchSize == 0 || forceFlush)
			{
				// Flush a batch of operations and release memory.
				if (session != null)
				{
					session.Flush(); // Flush to database.
					session.Clear(); // Clear level 1 cache.
					didFlush = true;
				}

				BatchCounter = 0;
			}

			return didFlush;
		}

		public bool ProcessBatch(BatchTransaction tx, bool forceCommit = false)
		{
			bool didCommit = false;

			++BatchCounter;

			if (BatchCounter % BatchSize == 0 || forceCommit)
			{
				// Commit a batch of operations and release memory.
				if (tx != null)
				{
					tx.CommitAndRenew(); // Commit to database.
					tx.Session.Clear(); // Clear level 1 cache.
					didCommit = true;
				}

				BatchCounter = 0;
			}

			return didCommit;
		}

		public BatchTransaction BeginTransaction(ISession session)
		{
			return new BatchTransaction(session);
		}
	}
}
