using Teltec.Storage.Versioning;

namespace Teltec.Storage.Backend
{
	public interface IPathBuilder
	{
		#region Local

		string LocalRootDirectory { get; set; }

		string CombineLocalPath(string localBaseDirectory, params string[] relativePaths);
		string BuildLocalPath(string remotePath);

		#endregion

		#region Remote

		string RemoteRootDirectory { get; set; }
		// Must reside within the RemoteRootDirectory, but outside the backup directories.
		string RemoteManifestDirectory { get; }

		string CombineRemotePath(string remoteBaseDirectory, params string[] relativePaths);
		string BuildRemotePath(string localPath);
		string BuildVersionedRemotePath(string localPath, IFileVersion version);

		#endregion
	}

	public abstract class PathBuilder : IPathBuilder
	{
		#region Local

		public string LocalRootDirectory { get; set; }

		public abstract string CombineLocalPath(string localBaseDirectory, params string[] relativePaths);
		public abstract string BuildLocalPath(string remotePath);

		#endregion

		#region Remote

		public string RemoteRootDirectory { get; set; }
		public virtual string RemoteManifestDirectory { get; protected set; }

		public abstract string CombineRemotePath(string remoteBaseDirectory, params string[] relativePaths);
		public abstract string BuildRemotePath(string localPath);
		public abstract string BuildVersionedRemotePath(string localPath, IFileVersion version);

		#endregion
	}
}
