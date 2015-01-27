using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Teltec.Common.Forms;

namespace Teltec.Common.Forms
{
	public partial class FileSystemTreeView : TreeView
	{
		public FileSystemTreeView()
		{
			InitializeComponent();
			PopulateTreeView();
			this.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.handle_AfterCheck);
			this.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.handle_BeforeExpand);
		}

		private const int WM_LBUTTONDBLCLK = 0x0203;
		private const int WM_RBUTTONDOWN = 0x0204;

		protected override void WndProc(ref Message m)
		{
			//
			// REFERENCES:
			//	http://stackoverflow.com/a/3174824/298054
			//	https://social.msdn.microsoft.com/Forums/windows/en-US/9d717ce0-ec6b-4758-a357-6bb55591f956/possible-bug-in-net-treeview-treenode-checked-state-inconsistent?forum=winforms
			//

			switch (m.Msg)
			{
				case WM_LBUTTONDBLCLK:
					{
						// Disable double-click on checkbox to fix Microsoft Vista bug.
						TreeViewHitTestInfo hitInfo = HitTest(new Point((int)m.LParam));
						if (hitInfo != null && hitInfo.Location == TreeViewHitTestLocations.StateImage)
						{
							m.Result = IntPtr.Zero;
							return;
						}
						break;
					}
				case WM_RBUTTONDOWN:
					{
						// Set focus to node on right-click - another Microsoft bug?
						TreeViewHitTestInfo hitInfo = HitTest(new Point((int)m.LParam));
						if (hitInfo != null)
							this.SelectedNode = hitInfo.Node;
						break;
					}
			}

			base.WndProc(ref m);
		}

		private void PopulateTreeView()
		{
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (var drive in drives)
			{
				TreeNode driveNode = AddDriveNode(this, drive);
				AddLazyLoadingNode(driveNode);
			}
		}

		private void PopulateDrive(TreeNode parentNode, DriveInfo parentDrive)
		{
			PopuplateDirectory(parentNode, parentDrive.RootDirectory);
		}

		private void PopuplateDirectory(TreeNode parentNode, DirectoryInfo parentDir)
		{
			try
			{
				DirectoryInfo[] subDirs = parentDir.GetDirectories();
				FileInfo[] subFiles = parentDir.GetFiles();
				foreach (DirectoryInfo subDir in subDirs)
				{
					TreeNode subFolderNode = AddFolderNode(parentNode, subDir);
					AddLazyLoadingNode(subFolderNode);
				}
				foreach (var file in subFiles)
				{
					TreeNode subFileNode = AddFileNode(parentNode, file);
				}
			}
			catch (System.SystemException e)
			{
				ShowErrorMessage(this, parentNode, e);
			}
		}

		private void ShowErrorMessage(TreeView view, TreeNode node, Exception exception)
		{
			MessageBox.Show(view, exception.Message, "Error");
			node.Checked = false;
			node.Collapse();
		}

		private TreeNode AddDriveNode(TreeView view, DriveInfo drive)
		{
			String nodeName = null;
			try
			{
				nodeName = String.Format("{0} ({1})", drive.VolumeLabel, drive.Name);
			}
			catch (Exception)
			{
				nodeName = drive.Name;
			}
			TreeNode node = new TreeNode(nodeName, 0, 0);
			node.Tag = drive;
			node.ImageKey = "drive";
			view.Nodes.Add(node);
			return node;
		}

		private TreeNode AddFolderNode(TreeNode parentNode, DirectoryInfo folder)
		{
			TreeNode node = new TreeNode(folder.Name, 0, 0);
			node.Tag = folder;
			node.ImageKey = "folder";
			if (parentNode.Checked)
				node.Checked = true;
			parentNode.Nodes.Add(node);
			return node;
		}

		private TreeNode AddFileNode(TreeNode parentNode, FileInfo file)
		{
			TreeNode node = new TreeNode(file.Name, 0, 0);
			node.Tag = file;
			node.ImageKey = "file";
			if (parentNode.Checked)
				node.Checked = true;
			parentNode.Nodes.Add(node);
			return node;
		}

		private TreeNode AddLazyLoadingNode(TreeNode parentNode)
		{
			TreeNode node = new TreeNode("Retrieving data...", 0, 0);
			node.Tag = null;
			node.ImageKey = "loading";
			parentNode.Nodes.Add(node);
			return node;
		}

		private void RemoveLazyLoadingNode(TreeNode parentNode)
		{
			foreach (TreeNode node in parentNode.Nodes)
			{
				if (node.ImageKey == "loading")
					node.Remove();
			}
		}

		private void CheckNodesRecursively(TreeNode node)
		{
			if (node.Tag == null)
				return;

			//Console.WriteLine("checking {0}", node.Text);
			if (!node.Checked)
				node.Checked = true;

			node.ForeColor = Color.Black;

			foreach (TreeNode subNode in node.Nodes)
				CheckNodesRecursively(subNode);
		}

		private void UncheckNodesRecursively(TreeNode node)
		{
			if (node.Tag == null)
				return;

			//Console.WriteLine("unchecking {0}", node.Text);
			if (node.Checked)
				node.Checked = false;

			node.ForeColor = Color.Black;

			foreach (TreeNode subNode in node.Nodes)
				UncheckNodesRecursively(subNode);
		}

		private void UpdateParentNodes(TreeNode changedNode, int countCheckedNodes)
		{
			if (changedNode.Parent == null)
				return;

			if (countCheckedNodes > 0)
			{
				changedNode.Parent.ForeColor = Color.LightGray;
				UpdateParentNodes(changedNode.Parent, countCheckedNodes);
			}
			else
			{
				int updatedCountCheckedNodes = CountCheckedNodes(changedNode.Parent);
				if (updatedCountCheckedNodes > 0 && !changedNode.Checked)
					changedNode.Parent.ForeColor = Color.LightGray;
				else
					changedNode.Parent.ForeColor = Color.Black;
				UpdateParentNodes(changedNode.Parent, updatedCountCheckedNodes);
			}
		}

		private int CountCheckedNodes(TreeNode parentNode)
		{
			if (parentNode == null)
				return 0;

			int count = 0;
			foreach (TreeNode node in parentNode.Nodes)
			{
				if (node.Checked)
				{
					count++;
				}
				else if (node.Nodes.Count > 0)
				{
					count += CountCheckedNodes(node);
				}
			}
			return count;
		}

		private void handle_AfterCheck(object sender, TreeViewEventArgs e)
		{
			OnFileSystemFetchStarted(this, null);
			TreeNode changedNode = e.Node;
			this.AfterCheck -= handle_AfterCheck;
			if (changedNode.Checked)
			{
				CheckNodesRecursively(changedNode);
				UpdateParentNodes(changedNode, CountCheckedNodes(changedNode.Parent));
			}
			else
			{
				UncheckNodesRecursively(changedNode);
				UpdateParentNodes(changedNode, CountCheckedNodes(changedNode.Parent));
			}
			this.AfterCheck += handle_AfterCheck;
			OnFileSystemFetchEnded(this, null);
		}

		private void handle_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			OnFileSystemFetchStarted(this, null);
			//Thread.Sleep(2000);
			TreeNode expandingNode = e.Node;
			if (expandingNode.ImageKey == "folder")
			{
				DirectoryInfo expandingNodeInfo = expandingNode.Tag as DirectoryInfo;
				RemoveLazyLoadingNode(expandingNode);
				if (expandingNode.Nodes.Count == 0)
					PopuplateDirectory(expandingNode, expandingNodeInfo);
			}
			else if (expandingNode.ImageKey == "drive")
			{
				DriveInfo expandingNodeInfo = expandingNode.Tag as DriveInfo;
				RemoveLazyLoadingNode(expandingNode);
				if (expandingNode.Nodes.Count == 0)
					PopulateDrive(expandingNode, expandingNodeInfo);
			}
			OnFileSystemFetchEnded(this, null);
		}

		#region Custom events

		public delegate void FileSystemFetchStartedHandler(object sender, EventArgs e);
		public delegate void FileSystemFetchEndedHandler(object sender, EventArgs e);

		public event FileSystemFetchStartedHandler FileSystemFetchStarted;
		public event FileSystemFetchEndedHandler FileSystemFetchEnded;

		protected virtual void OnFileSystemFetchStarted(object sender, EventArgs e)
		{
			if (FileSystemFetchStarted != null)
				FileSystemFetchStarted(this, e);
		}

		protected virtual void OnFileSystemFetchEnded(object sender, EventArgs e)
		{
			if (FileSystemFetchEnded != null)
				FileSystemFetchEnded(this, e);
		}

		#endregion
	}
}
