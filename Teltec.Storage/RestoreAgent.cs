using System.Threading.Tasks;
using Teltec.Storage.Agent;
using Teltec.Storage.Versioning;

namespace Teltec.Storage
{
	public class RestoreAgent<T> : AbstractAgent<T> where T : IVersionedFile
	{
		public RestoreAgent(IAsyncTransferAgent agent)
			: base(agent)
		{
			RegisterDownloadEventHandlers();
		}

		public override async Task DoImplementation(IVersionedFile file, object userData)
		{
			await TransferAgent.DownloadVersionedFile(file.Path, file.Version, userData);
		}
	}
}
