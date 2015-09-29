
namespace Teltec.Storage
{
	public delegate void PathScannerFileAddedHandler<T>(object sender, T file) where T : class;

	public interface IPathScanner<T> where T : class
	{
		PathScanResults<T> Results { get; }
		PathScannerFileAddedHandler<T> FileAdded { get; set; }
		void Scan();
	}

	public abstract class PathScanner<T> : IPathScanner<T> where T : class
	{
		public PathScanResults<T> Results { get; protected set; }
		public PathScannerFileAddedHandler<T> FileAdded { get; set; }
		public abstract void Scan();
	}
}
