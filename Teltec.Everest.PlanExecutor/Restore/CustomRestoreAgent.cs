using Teltec.Everest.Data.Versioning;
using Teltec.Storage;
using Teltec.Storage.Backend;

namespace Teltec.Everest.PlanExecutor.Restore
{
	public class CustomRestoreAgent : RestoreAgent<CustomVersionedFile>
	{
		public CustomRestoreAgent(ITransferAgent agent)
			: base(agent)
		{
		}
	}
}
