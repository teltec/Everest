using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teltec.Everest.Logging
{
	public sealed class LoggingHelper
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static void ChangeFilenamePostfix(string postfix)
		{
			var target = (FileTarget)LogManager.Configuration.FindTargetByName("logfile");
			if (target == null)
			{
				logger.Warn("Couldn't find log target named \"logfile\"");
				return;
			}

			target.FileName = string.Format("${{basedir}}/logs/${{shortdate}}-{0}.log", postfix);
			target.KeepFileOpen = true;

			LogManager.ReconfigExistingLoggers();
		}
	}
}
