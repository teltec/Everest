using Teltec.Storage;
using Teltec.Storage.Backend;

namespace DemoTransferS3
{
	public class CustomRestoreAgent : RestoreAgent<CustomVersionedFile>
	{
		public CustomRestoreAgent(ITransferAgent agent)
			: base(agent)
		{
		}
	}
}
