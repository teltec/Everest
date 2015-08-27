using System.Collections.Generic;

namespace Teltec.Storage
{
	public class PathScanResults<T> : IResults
	{
		public class Statistics
		{
			private int _Scanned = 0;
			private int _Failed = 0;

			public int Scanned { get { return _Scanned; } set { _Scanned = value; } }
			public int Failed { get { return _Failed; } set { _Failed = value; } }

			internal void Reset()
			{
				_Scanned = 0;
				_Failed = 0;
			}
		}

		//private IPathScanMonitor _Monitor;
		//public IPathScanMonitor Monitor
		//{
		//	get { return _Monitor; }
		//	set { _Monitor = value; }
		//}

		public Statistics Stats { get; private set; }

		public LinkedList<T> Files { get; set; }

		public Dictionary<string /* path */, string /* message */> FailedFiles { get; private set; }

		public PathScanResults()
		{
			Stats = new Statistics();
			Files = new LinkedList<T>();
			FailedFiles = new Dictionary<string, string>();
		}

		public void AddedFile(T file)
		{
			Files.AddLast(file);
			Stats.Scanned++;
		}

		public void FailedFile(string path, string message)
		{
			FailedFiles.Add(path, message);
			Stats.Failed++;
		}
	}
}
