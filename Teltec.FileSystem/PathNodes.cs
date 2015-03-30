using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teltec.FileSystem
{
	public class PathNodes
	{
		PathComponents Components;
		IEnumerable<PathNode> Nodes;

		public PathNodes(string path)
		{
			Components = new PathComponents(path);
			Nodes = GetParentNodes(Components);
		}

		public IEnumerable<PathNode> Parents
		{
			get { return Nodes; }
		}

		public PathNode Parent
		{
			get { return Nodes.Last(); }
		}

		protected string FormattedJoinedDirectoriesUptTo(PathComponents comps, int amount, bool includeDrive = true)
		{
			if (!comps.HasDirectories || amount <= 0)
				return string.Empty;

			return (includeDrive ? FormattedDrive(comps) : string.Empty)
				+ string.Join(Path.DirectorySeparatorChar.ToString(), comps.Directories.Take(amount));
		}

		protected string FormattedDrive(PathComponents comps)
		{
			return comps.HasDrive ? comps.Drive + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar : string.Empty;
		}

		protected IEnumerable<PathNode> GetParentNodes(PathComponents comps)
		{
			LinkedList<PathNode> nodes = new LinkedList<PathNode>();
			PathNode previousNode = null;
			PathNode currentNode = null;

			if (!comps.HasFileName && !comps.HasDirectories)
				// No filename? No directories?
				// The DRIVE itself has no intermediates, so return nothing.
				return nodes;

			if (comps.HasDrive)
			{
				currentNode = new PathNode(PathNode.TypeEnum.DRIVE, FormattedDrive(comps), previousNode);
				previousNode = currentNode;
				nodes.AddLast(currentNode); // Include drive
			}

			if (!comps.HasDirectories)
				return nodes;

			int count = comps.Directories.Length; // Includes ALL directories
			if (!comps.HasFileName)
				count--; // Exclude the most inner directory.

			for (int i = 1; i <= count; i++)
			{
				currentNode = new PathNode(PathNode.TypeEnum.FOLDER, FormattedJoinedDirectoriesUptTo(comps, i), previousNode);
				previousNode = currentNode;
				nodes.AddLast(currentNode);
			}

			return nodes;
		}
	}
}
