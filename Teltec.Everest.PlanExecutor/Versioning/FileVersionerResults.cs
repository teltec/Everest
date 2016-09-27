/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using Teltec.Common;

namespace Teltec.Everest.PlanExecutor.Versioning
{
	public class FileVersionerResults
	{
		public class Statistics
		{
			private int _Total = 0;
			private int _Failed = 0;

			public int Total { get { return _Total; } set { _Total = value; } }
			public int Failed { get { return _Failed; } set { _Failed = value; } }

			private long _BytesTotal = 0;

			public long BytesTotal { get { return _BytesTotal; } set { _BytesTotal = value; } }

			internal void Reset(int pending)
			{
				_Total = pending;
				_Failed = 0;
				_BytesTotal = 0;
			}
		}

		public Statistics Stats { get; private set; }

		public List<string> ErrorMessages { get; private set; }

		public FileVersionerResults()
		{
			Stats = new Statistics();
			ErrorMessages = new List<string>();
		}

		internal void Reset()
		{
			Stats.Reset(0);
			ErrorMessages.Clear();
		}

		#region Events

		internal void OnFileCompleted(object sender, FileVersionerEventArgs args)
		{
			Stats.Total++;
			Stats.BytesTotal += args.FileSize;
		}

		internal void OnFileFailed(object sender, FileVersionerEventArgs args, string message)
		{
			Stats.Total++;
			Stats.Failed++;
			OnError(sender, message);
		}

		internal void OnError(object sender, string message)
		{
			ErrorMessages.Add(message);
		}

		#endregion
	}

	public class FileVersionerEventArgs : ObservableEventArgs
	{
		private string _FilePath;
		public string FilePath
		{
			get { return _FilePath; }
			set { SetField(ref _FilePath, value); }
		}

		private long _FileSize; // In bytes
		public long FileSize
		{
			get { return _FileSize; }
			set { SetField(ref _FileSize, value); }
		}
	}
}
