using System;
using System.IO;
using System.Windows.Forms;

namespace Teltec.Backup.App.Controls
{
	public class BackupPlanTreeNode : TreeNode
	{
		public BackupPlanTreeNodeData Data { get; set; }

		public enum TypeEnum
		{
			LOADING = 0,
			DRIVE = 1,
			FOLDER = 2,
			FILE = 3,
			FILE_VERSION = 4,
		}

		public static BackupPlanTreeNode CreateLoadingNode()
		{
			BackupPlanTreeNode node = new BackupPlanTreeNode("Retrieving data...", 0, 0);
			node.Data.Type = TypeEnum.LOADING;
			node.ImageKey = "loading";
			return node;
		}

		public static BackupPlanTreeNode CreateDriveNode(DriveInfo info)
		{
			string nodeName = null;
			try
			{
				string driveLabel = info.VolumeLabel;
				if (string.IsNullOrEmpty(driveLabel))
					nodeName = info.Name;
				else
					nodeName = string.Format("{0} ({1})", info.Name, driveLabel);
			}
			catch (Exception)
			{
				nodeName = info.Name;
			}

			BackupPlanTreeNode node = new BackupPlanTreeNode(nodeName, 0, 0);
			node.Data.Type = TypeEnum.DRIVE;
			node.ImageKey = "drive";
			node.Data.InfoObject = info;
			node.AddLazyLoadingNode();
			return node;
		}

		public static BackupPlanTreeNode CreateFolderNode(DirectoryInfo info)
		{
			string nodeName = info.Name;
			BackupPlanTreeNode node = new BackupPlanTreeNode(nodeName, 0, 0);
			node.Data.Type = TypeEnum.FOLDER;
			node.ImageKey = "folder";
			node.Data.InfoObject = info;
			node.AddLazyLoadingNode();
			return node;
		}

		public static BackupPlanTreeNode CreateFileNode(FileInfo info)
		{
			string nodeName = info.Name;
			BackupPlanTreeNode node = new BackupPlanTreeNode(nodeName, 0, 0);
			node.Data.Type = TypeEnum.FILE;
			node.ImageKey = "file";
			node.Data.InfoObject = info;
			return node;
		}

		//public static BackupPlanTreeNode CreateFileVersionNode(FileVersionInfo info)
		//{
		//	string nodeName = info.Name;
		//	BackupPlanTreeNode node = new BackupPlanTreeNode(nodeName, 0, 0);
		//	node.Data.Type = TypeEnum.FILE_VERSION;
		//	node.ImageKey = "version";
		//	node.Data.InfoObject = info;
		//	return node;
		//}

		private BackupPlanTreeNode()
			: base()
		{
			Data = new BackupPlanTreeNodeData();
		}

		private BackupPlanTreeNode(string text, int imageIndex, int selectedImageIndex)
			: base(text, imageIndex, selectedImageIndex)
		{
			Data = new BackupPlanTreeNodeData();
		}

		#region Handle children nodes

		// May throw System.SystemException
		public void OnExpand()
		{
			RemoveLazyLoadingNode();

			switch (Data.Type)
			{
				default:
					break;
				//case BackupPlanTreeNode.TypeEnum.FOLDER:
				//	{
				//		DirectoryInfo expandingNodeInfo = Data.InfoObject as DirectoryInfo;
				//		if (Nodes.Count == 0)
				//			PopuplateDirectory(expandingNodeInfo);
				//		break;
				//	}
				//case BackupPlanTreeNode.TypeEnum.FILE:
				//	{
				//		FileInfo expandingNodeInfo = Data.InfoObject as FileInfo;
				//		if (Nodes.Count == 0)
				//			PopuplateFile(expandingNodeInfo);
				//		break;
				//	}
				//case BackupPlanTreeNode.TypeEnum.DRIVE:
				//	{
				//		DriveInfo expandingNodeInfo = Data.InfoObject as DriveInfo;
				//		if (Nodes.Count == 0)
				//			PopulateDrive(expandingNodeInfo);
				//		break;
				//	}
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

			if (node.Data.Type == BackupPlanTreeNode.TypeEnum.LOADING)
				node.Remove();
		}

		#endregion

/*
		#region Populate methods

		// May throw System.SystemException
		private void PopulateDrive(DriveInfo info)
		{
			PopuplateDirectory(info.RootDirectory);
		}

		// May throw System.SystemException
		private void PopuplateDirectory(DirectoryInfo info)
		{
			DirectoryInfo[] subDirs = info.GetDirectories();
			FileInfo[] subFiles = info.GetFiles();
			foreach (DirectoryInfo subDir in subDirs)
			{
				BackupPlanTreeNode subFolderNode = AddFolderNode(subDir);
			}
			foreach (var file in subFiles)
			{
				BackupPlanTreeNode subFileNode = AddFileNode(file);
			}
		}

		// May throw System.SystemException
		private void PopuplateFile(FileInfo info)
		{
			throw new NotImplementedException();
		}

		private BackupPlanTreeNode AddDriveNode(TreeView view, DriveInfo info)
		{
			BackupPlanTreeNode node = BackupPlanTreeNode.CreateDriveNode(info);
			view.Nodes.Add(node);
			return node;
		}

		private BackupPlanTreeNode AddFolderNode(DirectoryInfo info)
		{
			BackupPlanTreeNode node = BackupPlanTreeNode.CreateFolderNode(info);
			Nodes.Add(node);
			return node;
		}

		private BackupPlanTreeNode AddFileNode(FileInfo info)
		{
			BackupPlanTreeNode node = BackupPlanTreeNode.CreateFileNode(info);
			Nodes.Add(node);
			return node;
		}

		private BackupPlanTreeNode AddFileVersionNode(FileVersionInfo info)
		{
			BackupPlanTreeNode node = BackupPlanTreeNode.CreateFileVersionNode(info);
			Nodes.Add(node);
			return node;
		}

		#endregion
*/
	}
}
