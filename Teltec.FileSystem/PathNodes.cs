/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Teltec.FileSystem
{
	public class PathNodes
	{
		PathComponents Components;

		public PathNodes(string path)
		{
			Components = new PathComponents(path);
			Nodes = Split(Components);
		}

		IEnumerable<PathNode> _Nodes;
		public IEnumerable<PathNode> Nodes
		{
			get { return _Nodes; }
			protected set { _Nodes = value; }
		}

		public bool HasNodes
		{
			get { return Nodes.Count() > 0; }
		}

		public PathNode FirstNode
		{
			get { return HasNodes ? Nodes.First() : null; }
		}

		public PathNode LastNode
		{
			get { return HasNodes ? Nodes.Last() : null; }
		}

		public PathNode ParentNode
		{
			get { return Nodes.Count() > 1 ? Nodes.Last().Parent : null; }
		}

		protected string FormattedJoinedDirectoriesUptTo(PathComponents comps, int amount, bool includeDrive = true)
		{
			if (!comps.HasDirectories || amount <= 0 || amount > comps.Directories.Length)
				return string.Empty;

			return (includeDrive ? FormattedDrivePath(comps) : string.Empty)
				+ string.Join(Path.DirectorySeparatorChar.ToString(), comps.Directories.Take(amount))
				+ Path.DirectorySeparatorChar.ToString();
		}

		protected string FormattedDriveName(PathComponents comps)
		{
			return comps.HasDrive ? comps.Drive + Path.VolumeSeparatorChar : string.Empty;
		}

		protected string FormattedDrivePath(PathComponents comps)
		{
			return comps.HasDrive ? comps.Drive + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar : string.Empty;
		}

		protected IEnumerable<PathNode> Split(PathComponents comps)
		{
			LinkedList<PathNode> nodes = new LinkedList<PathNode>();
			PathNode previousNode = null;
			PathNode currentNode = null;

			string nodeName = null;
			string nodePath = null;

			if (comps.HasDrive)
			{
				nodeName = FormattedDriveName(comps);
				nodePath = FormattedDrivePath(comps);
				currentNode = new PathNode(PathNode.TypeEnum.DRIVE, nodeName, nodePath, previousNode);
				previousNode = currentNode;
				nodes.AddLast(currentNode); // Include drive
			}

			if (comps.HasDirectories)
			{
				// Include ALL directories
				for (int i = 0; i < comps.Directories.Length; i++)
				{
					nodeName = comps.Directories.ElementAt(i);
					nodePath = FormattedJoinedDirectoriesUptTo(comps, i + 1);
					currentNode = new PathNode(PathNode.TypeEnum.FOLDER, nodeName, nodePath, previousNode);
					previousNode = currentNode;
					nodes.AddLast(currentNode); // Include directory
				}
			}

			if (comps.HasFileName)
			{
				nodeName = comps.FileName;
				nodePath = comps.FullPath;
				currentNode = new PathNode(PathNode.TypeEnum.FILE, nodeName, nodePath, previousNode);
				previousNode = currentNode;
				nodes.AddLast(currentNode); // Include file
			}

			return nodes;
		}
	}
}
