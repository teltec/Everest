using NLog;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using Teltec.Backup.App.Forms;
using Teltec.Storage;

namespace Teltec.Backup.App
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
			AsyncHelper.SettingsMaxThreadCount = Teltec.Backup.Settings.Properties.Current.MaxThreadCount;
		}
    }
}
