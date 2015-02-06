using SimpleInjector;
using System;
using System.Data.Entity;
using System.Windows.Forms;
using Teltec.Backup.Forms;
using Teltec.Backup.Models;

namespace Teltec.Backup
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
            Application.Run(new MainForm());
			Provider.Cleanup();
        }
    }
}
