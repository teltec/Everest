/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.Win32;
using NLog;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using Teltec.Everest.App.Forms;
using Teltec.Everest.Logging;
using Teltec.Storage;

namespace Teltec.Everest.App
{
    static class Program
    {
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			try
			{
				UnsafeMain();
			}
			catch (Exception ex)
			{
				if (Environment.UserInteractive)
				{
					string message = string.Format(
						"Caught a fatal exception ({0}). Check the log file for more details.",
						ex.Message);
					if (Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero)
						MessageBox.Show(message);
				}
				logger.Log(LogLevel.Fatal, ex, "Caught a fatal exception");
			}
        }

		static void UnsafeMain()
		{
			LoggingHelper.ChangeFilenamePostfix("gui");
			SystemEvents.SessionEnding += (object sender, SessionEndingEventArgs e) =>
			{
				logger.Info("Session ending due to {0}", e.Reason.ToString());
			};
			Provider.Setup();
			LoadSettings();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			MainForm mainForm = new MainForm();
			Application.Run(mainForm);
			Provider.Cleanup();
		}

		private static void LoadSettings()
		{
			AsyncHelper.SettingsMaxThreadCount = Teltec.Everest.Settings.Properties.Current.MaxThreadCount;
		}
    }
}
