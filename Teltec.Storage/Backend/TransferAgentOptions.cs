
namespace Teltec.Storage.Backend
{
	public class TransferAgentOptions
	{
		private long _UploadChunkSizeInBytes;
		public long UploadChunkSizeInBytes
		{
			get { return _UploadChunkSizeInBytes; }
			set { _UploadChunkSizeInBytes = value; }
		}
	}
}
