using System;
using System.Collections.Generic;
using Teltec.Storage.Versioning;

namespace Teltec.Storage
{
	public delegate void PathScannerFileAddedHandler<T>(object sender, T file) where T : IVersionedFile;

	public interface IPathScanner<T> where T : IVersionedFile
	{
		PathScannerFileAddedHandler<T> FileAdded { get; set; }
		LinkedList<T> Scan();
	}

	public abstract class PathScanner<T> : IPathScanner<T> where T : IVersionedFile
	{
		public PathScannerFileAddedHandler<T> FileAdded { get; set; }
		public abstract LinkedList<T> Scan();
	}
}
