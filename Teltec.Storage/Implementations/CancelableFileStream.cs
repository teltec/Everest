using System.IO;
using System.Threading;

namespace Teltec.Storage.Implementations
{
	public class CancelableFileStream : FileStream
	{
		private CancellationToken _CancellationToken;

		public CancelableFileStream(string path, FileMode mode, FileAccess access, CancellationToken cancellationToken)
			: base(path, mode, access)
		{
			_CancellationToken = cancellationToken;
		}

		public override int Read(byte[] array, int offset, int count)
		{
			if (_CancellationToken != null)
				_CancellationToken.ThrowIfCancellationRequested();
			return base.Read(array, offset, count);
		}

		public override void Write(byte[] array, int offset, int count)
		{
			if (_CancellationToken != null)
				_CancellationToken.ThrowIfCancellationRequested();
			base.Write(array, offset, count);
		}
	}
}
