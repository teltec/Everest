/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Teltec.Everest.Data.FileSystem;
using Teltec.Storage.Versioning;

namespace Teltec.Everest.App.Controls
{
	public class EntryInfo
	{
		public TypeEnum Type { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public IFileVersion Version { get; set; }

		public EntryInfo(TypeEnum type, string name, string path)
		{
			Type = type;
			Name = name;
			Path = path;
		}

		public EntryInfo(TypeEnum type, string name, string path, IFileVersion version)
			: this(type, name, path)
		{
			Version = version;
		}
	}
}
