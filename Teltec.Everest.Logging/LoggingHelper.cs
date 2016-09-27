/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NLog;
using NLog.Targets;

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
