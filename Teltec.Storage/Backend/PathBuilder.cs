using Teltec.Storage.Versioning;

namespace Teltec.Storage.Backend
{
	public interface IPathBuilder
	{
		string RootDirectory { get; set; }

		string BuildPath(string path);
		string BuildVersionedPath(string path, IFileVersion version);
	}

	public abstract class PathBuilder : IPathBuilder
	{
		public string RootDirectory { get; set; }

		public abstract string BuildPath(string path);
		public abstract string BuildVersionedPath(string path, IFileVersion version);
	}
}
