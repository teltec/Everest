
namespace Teltec.FileSystem
{
	public class PathNode
	{
		public TypeEnum Type { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public PathNode Parent { get; set; }

		public PathNode(TypeEnum type, string name, string path, PathNode parent = null)
		{
			Type = type;
			Name = name;
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
