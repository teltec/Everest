using System.Threading.Tasks;
using Teltec.Storage.Agent;
using Teltec.Storage.Versioning;

namespace Teltec.Storage
{
	public class SynchronizationAgent<T> : AbstractSyncAgent<T> where T : IVersionedFile
	{
		public SynchronizationAgent(IAsyncTransferAgent agent)
			: base(agent)
		{
			RegisterListingEventHandlers();
		}

		public override async Task DoImplementation(string prefix, bool recursive, object userData)
		{
			await TransferAgent.List(prefix, recursive, userData);
		}
	}
}

