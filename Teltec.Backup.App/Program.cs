using System;
using System.Windows.Forms;
using Teltec.Backup.App.Forms;
using Teltec.Storage;
namespace Teltec.Backup.App
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			Provider.Setup();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
			LoadSettings();
			Application.Run(new MainForm());
			Provider.Cleanup();
        }

		private static void LoadSettings()
		{
			AsyncHelper.SettingsMaxThreadCount = Teltec.Backup.Settings.Properties.Current.MaxThreadCount;
		}
    }
}
