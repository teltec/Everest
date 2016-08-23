using NHibernate;
using NUnit.Framework;
using Teltec.Everest.Data.DAO;
using Teltec.FileSystem;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.PlanExecutor
{
	public class BackupPlanPathNodeCreator
	{
		BackupPlanPathNodeRepository _dao;
		ITransaction _tx;

		public BackupPlanPathNodeCreator(BackupPlanPathNodeRepository dao, ITransaction tx)
		{
			Assert.NotNull(dao);
			Assert.NotNull(tx);

			_dao = dao;
			_tx = tx;
		}

		public Models.BackupPlanPathNode CreateOrUpdatePathNodes(Models.StorageAccount account, Models.BackupPlanFile file)
		{
			PathNodes pathNodes = new PathNodes(file.Path);

			bool nodeExists = true; // Start assuming it exists.
			Models.BackupPlanPathNode previousNode = null;
			Models.BackupPlanPathNode planPathNode = null;
			foreach (var pathNode in pathNodes.Nodes)
			{
				// If it does not exist, it does not make sense to lookup inner directories/files.
				if (nodeExists)
				{
					planPathNode = _dao.GetByStorageAccountAndTypeAndPath(
						account, Models.EntryTypeExtensions.ToEntryType(pathNode.Type), pathNode.Path);

					// If we couldn't find the current `Models.BackupPlanPathNode`, it's safe to assume the inner
					// directories/files don't exist either. From now on, all nodes will be created/inserted.
					if (planPathNode == null)
						nodeExists = false;
				}

				if (!nodeExists)
				{
					//BackupPlanFile planFile = daoBackupPlanFile.GetByPlanAndPath(Backup.BackupPlan, file.Path);
					//Assert.NotNull(planFile, string.Format("Required {0} not found in the database.", typeof(BackupPlanFile).Name))
					planPathNode = new Models.BackupPlanPathNode(file,
						Models.EntryTypeExtensions.ToEntryType(pathNode.Type),
						pathNode.Name, pathNode.Path, previousNode);

					if (previousNode != null)
					{
						planPathNode.Parent = previousNode;
						previousNode.SubNodes.Add(planPathNode);
					}


					_dao.Insert(_tx, planPathNode);
					_dao.Refresh(planPathNode);
				}

				previousNode = planPathNode;

				//session.Evict(planPathNode); // Force future queries to re-load it and its relationships.
			}

			return previousNode;
		}
	}
}
