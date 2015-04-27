using NUnit.Framework;
using System;
using System.IO;
using System.Windows.Forms;
using Teltec.Backup.App.Models;

namespace Teltec.Backup.App.Controls
{
	public sealed class FileSystemTreeNode : EntryTreeNode
	{
		private FileSystemTreeNodeData _Data = new FileSystemTreeNodeData();
		public FileSystemTreeNodeData Data
		{
			get { return _Data;  }
			set { _Data = value; }
		}

		public static FileSystemTreeNode CreateLoadingNode()
		{
			FileSystemTreeNode node = new FileSystemTreeNode("Retrieving data...", 0, 0);
			node.Data.Type = TypeEnum.LOADING;
			node.ImageKey = "loading";
			return node;
		}

		public static FileSystemTreeNode CreateDriveNode(EntryInfo info)
		{
			//string nodeName = null;
			// TODO: Support showing drive label!
			//try
			//{
			//	string driveLabel = info.VolumeLabel;
			//	if (string.IsNullOrEmpty(driveLabel))
			//		nodeName = info.Name;
			//	else
			//		nodeName = string.Format("{0} ({1})", info.Name, driveLabel);
			//}
			//catch (Exception)
			//{
			//	nodeName = info.Name;
			//}

			FileSystemTreeNode node = new FileSystemTreeNode(info.Name, 0, 0);
			node.ImageKey = "drive";
			node.Data.InfoObject = info;
			node.AddLazyLoadingNode();
			return node;
		}

		public static FileSystemTreeNode CreateFolderNode(EntryInfo info)
		{
			FileSystemTreeNode node = new FileSystemTreeNode(info.Name, 0, 0);
			node.ImageKey = "folder";
			node.Data.InfoObject = info;
			node.AddLazyLoadingNode();
			return node;
		}

		public static FileSystemTreeNode CreateFileNode(EntryInfo info)
		{
			FileSystemTreeNode node = new FileSystemTreeNode(info.Name, 0, 0);
			node.ImageKey = "file";
			node.Data.InfoObject = info;
			return node;
		}

		private FileSystemTreeNode()
			: base()
		{
		}

		private FileSystemTreeNode(string text, int imageIndex, int selectedImageIndex)
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
			FileSystemTreeNode node = FileSystemTreeNode.CreateLoadingNode();
			Nodes.Add(node);
			return node;
		}

		private void RemoveLazyLoadingNode()
		{
			FileSystemTreeNode node = FirstNode as FileSystemTreeNode;
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
			PopuplateDirectory(info);
		}

		// May throw System.SystemException
		private void PopuplateDirectory(EntryInfo info)
		{
			if (info.Type != TypeEnum.DRIVE && info.Type != TypeEnum.FOLDER)
				throw new ArgumentException("Unexpected TypeEnum", "info.Type");

			DirectoryInfo dir = info.Type == TypeEnum.DRIVE
				? new DriveInfo(info.Path).RootDirectory
				: new DirectoryInfo(info.Path);
				
			DirectoryInfo[] subDirs = dir.GetDirectories();
			FileInfo[] subFiles = dir.GetFiles();
			foreach (DirectoryInfo subDir in subDirs)
			{
				EntryInfo subInfo = new EntryInfo(TypeEnum.FOLDER, subDir.Name, subDir.FullName + System.IO.Path.DirectorySeparatorChar);
				FileSystemTreeNode subFolderNode = AddFolderNode(subInfo);
			}
			foreach (var file in subFiles)
			{
				EntryInfo subInfo = new EntryInfo(TypeEnum.FILE, file.Name, file.FullName);
				FileSystemTreeNode subFileNode = AddFileNode(subInfo);
			}
		}

		private FileSystemTreeNode AddDriveNode(TreeView view, EntryInfo info)
		{
			FileSystemTreeNode node = FileSystemTreeNode.CreateDriveNode(info);
			view.Nodes.Add(node);
			return node;
		}

		private FileSystemTreeNode AddFolderNode(EntryInfo info)
		{
			FileSystemTreeNode node = FileSystemTreeNode.CreateFolderNode(info);
			Nodes.Add(node);
			return node;
		}

		private FileSystemTreeNode AddFileNode(EntryInfo info)
		{
			FileSystemTreeNode node = FileSystemTreeNode.CreateFileNode(info);
			Nodes.Add(node);
			return node;
		}

		#endregion
	}
}
