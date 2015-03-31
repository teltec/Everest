using System;
using System.IO;
using System.Windows.Forms;

namespace Teltec.Common.Forms
{
	public class FileSystemTreeNode : TreeNode
	{
		public FileSystemTreeNodeData Data { get; set; }

		public enum TypeEnum
		{
			LOADING = 0,
			DRIVE = 1,
			FOLDER = 2,
			FILE = 3,
		}

		public static FileSystemTreeNode CreateLoadingNode()
		{
			FileSystemTreeNode node = new FileSystemTreeNode("Retrieving data...", 0, 0);
			node.Data.Type = TypeEnum.LOADING;
			node.ImageKey = "loading";
			return node;
		}

		public static FileSystemTreeNode CreateDriveNode(DriveInfo info)
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

			FileSystemTreeNode node = new FileSystemTreeNode(nodeName, 0, 0);
			node.Data.Type = TypeEnum.DRIVE;
			node.ImageKey = "drive";
			node.Data.InfoObject = info;
			node.AddLazyLoadingNode();
			return node;
		}

		public static FileSystemTreeNode CreateFolderNode(DirectoryInfo info)
		{
			string nodeName = info.Name;
			FileSystemTreeNode node = new FileSystemTreeNode(nodeName, 0, 0);
			node.Data.Type = TypeEnum.FOLDER;
			node.ImageKey = "folder";
			node.Data.InfoObject = info;
			node.AddLazyLoadingNode();
			return node;
		}

		public static FileSystemTreeNode CreateFileNode(FileInfo info)
		{
			string nodeName = info.Name;
			FileSystemTreeNode node = new FileSystemTreeNode(nodeName, 0, 0);
			node.Data.Type = TypeEnum.FILE;
			node.ImageKey = "file";
			node.Data.InfoObject = info;
			return node;
		}

		private FileSystemTreeNode()
			: base()
		{
			Data = new FileSystemTreeNodeData();
		}

		private FileSystemTreeNode(string text, int imageIndex, int selectedImageIndex)
			: base(text, imageIndex, selectedImageIndex)
		{
			Data = new FileSystemTreeNodeData();
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
				case FileSystemTreeNode.TypeEnum.FOLDER:
					{
						DirectoryInfo expandingNodeInfo = Data.InfoObject as DirectoryInfo;
						if (Nodes.Count == 0)
							PopuplateDirectory(expandingNodeInfo);
						break;
					}
				case FileSystemTreeNode.TypeEnum.DRIVE:
					{
						DriveInfo expandingNodeInfo = Data.InfoObject as DriveInfo;
						if (Nodes.Count == 0)
							PopulateDrive(expandingNodeInfo);
						break;
					}
			}
		}

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

			if (node.Data.Type == FileSystemTreeNode.TypeEnum.LOADING)
				node.Remove();
		}

		#endregion

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
				FileSystemTreeNode subFolderNode = AddFolderNode(subDir);
			}
			foreach (var file in subFiles)
			{
				FileSystemTreeNode subFileNode = AddFileNode(file);
			}
		}

		private FileSystemTreeNode AddDriveNode(TreeView view, DriveInfo drive)
		{
			FileSystemTreeNode node = FileSystemTreeNode.CreateDriveNode(drive);
			view.Nodes.Add(node);
			return node;
		}

		private FileSystemTreeNode AddFolderNode(DirectoryInfo folder)
		{
			FileSystemTreeNode node = FileSystemTreeNode.CreateFolderNode(folder);
			Nodes.Add(node);
			return node;
		}

		private FileSystemTreeNode AddFileNode(FileInfo file)
		{
			FileSystemTreeNode node = FileSystemTreeNode.CreateFileNode(file);
			Nodes.Add(node);
			return node;
		}

		#endregion
	}
}
