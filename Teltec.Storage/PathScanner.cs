using System.Collections.Generic;

namespace Teltec.Storage
{
	public delegate void PathScannerFileAddedHandler<T>(object sender, T file) where T : class;

	public interface IPathScanner<T> where T : class
	{
		PathScannerFileAddedHandler<T> FileAdded { get; set; }
		LinkedList<string> Scan();
	}

	public abstract class PathScanner<T> : IPathScanner<T> where T : class
	{
		public PathScannerFileAddedHandler<T> FileAdded { get; set; }
		public abstract LinkedList<string> Scan();
	}
}
