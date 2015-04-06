using NUnit.Framework;
using System;
using System.IO;
using System.Windows.Forms;
using Teltec.Backup.App.Models;

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

		public static BackupPlanTreeNode CreateDriveNode(EntryInfo info)
		{
			Assert.AreEqual(EntryType.DRIVE, info.Type);
			BackupPlanTreeNode node = new BackupPlanTreeNode(info.Name, 0, 0);
			node.ImageKey = "drive";
			node.Data.InfoObject = info;
			node.AddLazyLoadingNode();
			return node;
		}

		public static BackupPlanTreeNode CreateFolderNode(EntryInfo info)
		{
			Assert.AreEqual(EntryType.FOLDER, info.Type);
			BackupPlanTreeNode node = new BackupPlanTreeNode(info.Name, 0, 0);
			node.ImageKey = "folder";
			node.Data.InfoObject = info;
			node.AddLazyLoadingNode();
			return node;
		}

		public static BackupPlanTreeNode CreateFileNode(EntryInfo info)
		{
			Assert.AreEqual(EntryType.FILE, info.Type);
			BackupPlanTreeNode node = new BackupPlanTreeNode(info.Name, 0, 0);
			node.ImageKey = "file";
			node.Data.InfoObject = info;
			node.AddLazyLoadingNode();
			return node;
		}

		public static BackupPlanTreeNode CreateFileVersionNode(EntryInfo info)
		{
			Assert.AreEqual(EntryType.FILE_VERSION, info.Type);
			BackupPlanTreeNode node = new BackupPlanTreeNode(info.Name, 0, 0);
			node.ImageKey = "file_version";
			node.Data.InfoObject = info;
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

			switch (Data.Type)
			{
				default: break;
				case TypeEnum.FOLDER:
					if (Nodes.Count == 0)
						PopuplateDirectory(Data.InfoObject);
					break;
				case TypeEnum.FILE:
					if (Nodes.Count == 0)
						PopuplateFile(Data.InfoObject);
					break;
				case TypeEnum.DRIVE:
					if (Nodes.Count == 0)
						PopulateDrive(Data.InfoObject);
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
		private void PopulateDrive(EntryInfo info)
		{
			Assert.AreEqual(TypeEnum.DRIVE, info.Type);
			throw new NotImplementedException();
			//PopuplateDirectory(info.RootDirectory);
		}

		// May throw System.SystemException
		private void PopuplateDirectory(EntryInfo info)
		{
			Assert.AreEqual(TypeEnum.FOLDER, info.Type);
			throw new NotImplementedException();
			//DirectoryInfo[] subDirs = info.GetDirectories();
			//FileInfo[] subFiles = info.GetFiles();
			//foreach (DirectoryInfo subDir in subDirs)
			//{
			//	BackupPlanTreeNode subFolderNode = AddFolderNode(subDir);
			//}
			//foreach (var file in subFiles)
			//{
			//	BackupPlanTreeNode subFileNode = AddFileNode(file);
			//}
		}

		// May throw System.SystemException
		private void PopuplateFile(EntryInfo info)
		{
			Assert.AreEqual(TypeEnum.FILE, info.Type);
			throw new NotImplementedException();
		}

		private BackupPlanTreeNode AddDriveNode(TreeView view, EntryInfo info)
		{
			Assert.AreEqual(TypeEnum.DRIVE, info.Type);
			BackupPlanTreeNode node = BackupPlanTreeNode.CreateDriveNode(info);
			view.Nodes.Add(node);
			return node;
		}

		private BackupPlanTreeNode AddFolderNode(EntryInfo info)
		{
			Assert.AreEqual(TypeEnum.FOLDER, info.Type);
			BackupPlanTreeNode node = BackupPlanTreeNode.CreateFolderNode(info);
			Nodes.Add(node);
			return node;
		}

		private BackupPlanTreeNode AddFileNode(EntryInfo info)
		{
			Assert.AreEqual(TypeEnum.FILE, info.Type);
			BackupPlanTreeNode node = BackupPlanTreeNode.CreateFileNode(info);
			Nodes.Add(node);
			return node;
		}

		private BackupPlanTreeNode AddFileVersionNode(EntryInfo info)
		{
			Assert.AreEqual(TypeEnum.FILE_VERSION, info.Type);
			BackupPlanTreeNode node = BackupPlanTreeNode.CreateFileVersionNode(info);
			Nodes.Add(node);
			return node;
		}

		#endregion
	}
}
