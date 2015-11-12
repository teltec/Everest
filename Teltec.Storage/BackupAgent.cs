using Teltec.Storage.Backend;
using Teltec.Storage.Versioning;

namespace Teltec.Storage
{
	public class BackupAgent<T> : AbstractAgent<T> where T : IVersionedFile
	{
		public BackupAgent(ITransferAgent agent)
			: base(agent)
		{
			RegisterUploadEventHandlers();
			RegisterDeleteEventHandlers();
		}

		public override void DoImplementation(IVersionedFile file, object userData)
		{
			TransferAgent.UploadVersionedFile(file.Path, file.Version, userData);
		}
	}
}
