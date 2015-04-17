using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teltec.Backup.App.DAO;
using Teltec.Backup.App.Models;
using Teltec.Storage.Versioning;

namespace Teltec.Backup.App.Controls
{
	public sealed class BackupPlanTreeNode : EntryTreeNode
	{
		private BackupPlanTreeNodeData _Data = new BackupPlanTreeNodeData();
		public BackupPlanTreeNodeData Data
		{
			get { return _Data; }
			set { _Data = value; }
		}

		public static BackupPlanTreeNode CreateLoadingNode()
		{
			BackupPlanTreeNode node = new BackupPlanTreeNode("Retrieving data...", 0, 0);
			node.Data.Type = TypeEnum.LOADING;
			node.ImageKey = "loading";
			return node;
		}

		public static BackupPlanTreeNode CreateDriveNode(BackupPlanPathNode pathNode)
		{
			Assert.AreEqual(EntryType.DRIVE, pathNode.Type);
			BackupPlanTreeNode node = new BackupPlanTreeNode(pathNode.Name, 0, 0);
			node.ImageKey = "drive";
			EntryInfo info = new EntryInfo(TypeEnum.DRIVE, pathNode.Name, pathNode.Path);
			node.Data.InfoObject = info;
			node.Data.UserObject = pathNode;
			node.AddLazyLoadingNode();
			return node;
		}

		public static BackupPlanTreeNode CreateFolderNode(BackupPlanPathNode pathNode)
		{
			Assert.AreEqual(EntryType.FOLDER, pathNode.Type);
			BackupPlanTreeNode node = new BackupPlanTreeNode(pathNode.Name, 0, 0);
			node.ImageKey = "folder";
			EntryInfo info = new EntryInfo(TypeEnum.FOLDER, pathNode.Name, pathNode.Path);
			node.Data.InfoObject = info;
			node.Data.UserObject = pathNode;
			node.AddLazyLoadingNode();
			return node;
		}

		public static BackupPlanTreeNode CreateFileNode(BackupPlanPathNode pathNode)
		{
			Assert.AreEqual(EntryType.FILE, pathNode.Type);
			BackupPlanTreeNode node = new BackupPlanTreeNode(pathNode.Name, 0, 0);
			node.ImageKey = "file";
			EntryInfo info = new EntryInfo(TypeEnum.FILE, pathNode.Name, pathNode.Path);
			node.Data.InfoObject = info;
			node.Data.UserObject = pathNode;
			node.AddLazyLoadingNode();
			return node;
		}

		public static BackupPlanTreeNode CreateFileVersionNode(BackupPlanPathNode pathNode, IFileVersion version)
		{
			Assert.AreEqual(EntryType.FILE, pathNode.Type);
			BackupPlanTreeNode node = new BackupPlanTreeNode(version.Name, 0, 0);
			node.ImageKey = "file_version";
			EntryInfo info = new EntryInfo(TypeEnum.FILE_VERSION, pathNode.Name, pathNode.Path, version);
			node.Data.InfoObject = info;
			node.Data.UserObject = pathNode;
			return node;
		}

		private BackupPlanTreeNode()
			: base()
		{
		}

		private BackupPlanTreeNode(string text, int imageIndex, int selectedImageIndex)
			: base(text, imageIndex, selectedImageIndex)
		{
		}

		#region Handle children nodes

		// May throw System.SystemException
		public override void OnExpand()
		{
			RemoveLazyLoadingNode();

			BackupPlanPathNode pathNode = Data.UserObject as BackupPlanPathNode;

			switch (Data.Type)
			{
				default: break;
				case TypeEnum.FOLDER:
					if (Nodes.Count == 0)
					{
						PopuplateDirectory(pathNode);
					}
					break;
				case TypeEnum.FILE:
					if (Nodes.Count == 0)
					{
						PopulateFile(pathNode);
					}
					break;
				case TypeEnum.DRIVE:
					if (Nodes.Count == 0)
					{
						PopulateDrive(pathNode);
					}
					break;
			}
		}

		#endregion

		#region Lazy node

		private TreeNode AddLazyLoadingNode()
		{
			BackupPlanTreeNode node = BackupPlanTreeNode.CreateLoadingNode();
			Nodes.Add(node);
			return node;
		}

		private void RemoveLazyLoadingNode()
		{
			BackupPlanTreeNode node = FirstNode as BackupPlanTreeNode;
			if (node == null)
				return;

			if (node.Data.Type == TypeEnum.LOADING)
				node.Remove();
		}

		#endregion

		#region Populate methods

		// May throw System.SystemException
		private void PopulateDrive(BackupPlanPathNode pathNode)
		{
			Assert.AreEqual(EntryType.DRIVE, pathNode.Type);

			PopuplateDirectory(pathNode);
		}

		// May throw System.SystemException
		private void PopuplateDirectory(BackupPlanPathNode pathNode)
		{
			//Assert.AreEqual(EntryType.FOLDER, pathNode.Type);
			if (pathNode.Type != EntryType.DRIVE && pathNode.Type != EntryType.FOLDER)
				throw new ArgumentException("Unexpected EntryType", "pathNode.Type");

			foreach (var subNode in pathNode.SubNodes)
			{
				if (subNode.Type != EntryType.FOLDER)
					continue;
				BackupPlanTreeNode subFolderNode = AddFolderNode(subNode);
			}
			foreach (var subNode in pathNode.SubNodes)
			{
				if (subNode.Type != EntryType.FILE)
					continue;
				BackupPlanTreeNode subFolderNode = AddFileNode(subNode);
			}
		}

		// May throw System.SystemException
		private void PopulateFile(BackupPlanPathNode pathNode)
		{
			Assert.AreEqual(EntryType.FILE, pathNode.Type);

#if true
			BackupedFileRepository dao = new BackupedFileRepository();
			IList<BackupedFile> backupedFiles = dao.GetCompletedByPlanAndPath(pathNode.BackupPlan, pathNode.Path);
			IEnumerable<IFileVersion> versions =
				from f in backupedFiles
				select new FileVersion { Name = f.Backup.VersionName, Version = f.Backup.Version };
#else
			// *** DO NOT USE THIS APPROACH BECAUSE IT WILL NOT SHOW A BACKUP VERSION THAT HAS JUST BEEN CREATED! ***
			// I know it's a problem related to the NHibernate caching mechanism, but I don't want to deal with it right now. Sorry! :-)
			IList<BackupedFile> backupedFiles = pathNode.PlanFile.Versions;
			IEnumerable<IFileVersion> versions =
				from f in backupedFiles
				where f.TransferStatus == Storage.TransferStatus.COMPLETED
				select new FileVersion { Name = f.Backup.VersionName, Version = f.Backup.Version };
#endif
			foreach (IFileVersion version in versions)
			{
				BackupPlanTreeNode versionNode = AddFileVersionNode(pathNode, version);
			}
		}

		private BackupPlanTreeNode AddDriveNode(TreeView view, BackupPlanPathNode pathNode)
		{
			Assert.AreEqual(EntryType.DRIVE, pathNode.Type);
			BackupPlanTreeNode node = BackupPlanTreeNode.CreateDriveNode(pathNode);
			view.Nodes.Add(node);
			return node;
		}

		private BackupPlanTreeNode AddFolderNode(BackupPlanPathNode pathNode)
		{
			Assert.AreEqual(EntryType.FOLDER, pathNode.Type);
			BackupPlanTreeNode node = BackupPlanTreeNode.CreateFolderNode(pathNode);
			Nodes.Add(node);
			return node;
		}

		private BackupPlanTreeNode AddFileNode(BackupPlanPathNode pathNode)
		{
			Assert.AreEqual(EntryType.FILE, pathNode.Type);
			BackupPlanTreeNode node = BackupPlanTreeNode.CreateFileNode(pathNode);
			Nodes.Add(node);
			return node;
		}

		private BackupPlanTreeNode AddFileVersionNode(BackupPlanPathNode pathNode, IFileVersion version)
		{
			Assert.AreEqual(EntryType.FILE, pathNode.Type);
			BackupPlanTreeNode node = BackupPlanTreeNode.CreateFileVersionNode(pathNode, version);
			Nodes.Add(node);
			return node;
		}

		#endregion
	}
}
