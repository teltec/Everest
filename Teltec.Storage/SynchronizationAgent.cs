using Teltec.Storage.Backend;
using Teltec.Storage.Versioning;

namespace Teltec.Storage
{
	public class SynchronizationAgent<T> : AbstractSyncAgent<T> where T : IVersionedFile
	{
		public SynchronizationAgent(ITransferAgent agent)
			: base(agent)
		{
			RegisterListingEventHandlers();
		}

		public override void DoImplementation(string prefix, bool recursive, object userData)
		{
			TransferAgent.List(prefix, recursive, userData);
		}
	}
}

