using System.Threading.Tasks;
using Teltec.Storage.Agent;
using Teltec.Storage.Versioning;

namespace Teltec.Storage
{
	public class BackupAgent<T> : AbstractAgent<T> where T : IVersionedFile
	{
		public BackupAgent(IAsyncTransferAgent agent)
			: base(agent)
		{
			RegisterUploadEventHandlers();
			RegisterDeleteEventHandlers();
		}

		public override async Task DoImplementation(IVersionedFile file, object userData)
		{
			await TransferAgent.UploadVersionedFile(file.Path, file.Version, userData);
		}
	}
}
