using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teltec.FileSystem
{
	public class PathNode
	{
		public TypeEnum Type { get; set; }
		public string Path { get; set; }
		public PathNode Parent { get; set; }

		public PathNode(TypeEnum type, string path, PathNode parent = null)
		{
			Type = type;
			Path = path;
			Parent = parent;
		}

		public enum TypeEnum
		{
			UNKNOWN = 0,
			DRIVE = 1,
			FOLDER = 2,
			FILE = 3,
		}
	}
}
