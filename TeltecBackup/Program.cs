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
            var container = new Container();

            // NOTE: instances that are declared as Single should be thread-safe in a multi-threaded environment
            container.RegisterSingle<DbContext, DatabaseContext>();
            //container.Register<IUserRepository, SqlUserRepository>();

            container.Verify();
            //DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
