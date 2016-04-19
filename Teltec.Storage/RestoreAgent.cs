using Teltec.Storage.Backend;
using Teltec.Storage.Versioning;

namespace Teltec.Storage
{
	public class RestoreAgent<T> : AbstractAgent<T> where T : IVersionedFile
	{
		public RestoreAgent(ITransferAgent agent)
			: base(agent)
		{
			RegisterDownloadEventHandlers();
		}

		public override void DoImplementation(IVersionedFile file, object userData)
		{
			TransferAgent.DownloadVersionedFile(file.Path, file.Version, userData);
		}
	}
}
