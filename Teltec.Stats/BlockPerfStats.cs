using NLog;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Teltec.Common.Utils;

namespace Teltec.Stats
{
	public class BlockPerfStats
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		Stopwatch Timer;
		string MemberName;
		string SourceFilePath;
		int SourceLineNumberCreated;
		int SourceLineNumberStarted;
		int SourceLineNumberEnded;
		string Identifier;

		public TimeSpan Duration
		{
			get;
			internal set;
		}

		public BlockPerfStats([CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			Timer = new Stopwatch();
			MemberName = memberName;
			SourceFilePath = sourceFilePath;
			SourceLineNumberCreated = sourceLineNumber;
		}

		public void Begin([CallerLineNumber] int sourceLineNumber = 0)
		{
			SourceLineNumberStarted = sourceLineNumber;

			Timer.Start();
			LogBegin();
		}

		public void Begin(string identifier, [CallerLineNumber] int sourceLineNumber = 0)
		{
			Identifier = identifier;

			Begin(sourceLineNumber);
		}

		public void End([CallerLineNumber] int sourceLineNumber = 0)
		{
			Timer.Stop();

			Duration = Timer.Elapsed;
			SourceLineNumberEnded = sourceLineNumber;

			LogEnd();

			Timer.Reset();
		}

		private void LogBegin()
		{
			logger.Info(
#if DEBUG
				"{0}:{1}:{2} - BEGIN {3}",
#else
				"{2} - BEGIN {3}",
#endif
				SourceFilePath, SourceLineNumberCreated, MemberName, Identifier);
		}

		private void LogEnd()
		{
			logger.Info(
#if DEBUG
				"{0}:{1}:{2} - ENDED {3} - TOOK {4}",
#else
				"{2} - ENDED {3} - TOOK {4}",
#endif
				SourceFilePath, SourceLineNumberCreated, MemberName, Identifier,
				TimeSpanUtils.GetReadableTimespan(Duration));
		}
	}
}
