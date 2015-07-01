using NHibernate;
using NUnit.Framework;
using Teltec.Backup.Data.DAO;
using Teltec.FileSystem;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.PlanExecutor
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

			Models.BackupPlanPathNode previousNode = null;
			foreach (var pathNode in pathNodes.Nodes)
			{
				Models.BackupPlanPathNode planPathNode = _dao.GetByStorageAccountAndTypeAndPath(
					account, Models.EntryTypeExtensions.ToEntryType(pathNode.Type), pathNode.Path);

				if (planPathNode == null)
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
				}

				previousNode = planPathNode;

				//session.Evict(planPathNode); // Force future queries to re-load it and its relationships.
			}

			return previousNode;
		}
	}
}
