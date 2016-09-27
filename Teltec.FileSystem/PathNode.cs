/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

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
