using System;
using System.ServiceProcess;
using System.Timers;

namespace Teltec.Backup.Svc
{
	public partial class Service : ServiceBase
	{
		static Timer timer;

		static void Main()
		{
			System.ServiceProcess.ServiceBase[] servicesToRun; 
			servicesToRun = new ServiceBase[] { new Service() };

			System.ServiceProcess.ServiceBase.Run(servicesToRun);
		}

		public Service()
		{
		}

		protected override void OnStart(string[] args)
		{
			timer = new Timer();
			timer.Interval = 2500; //1000 * 60 * 60 * 24; // Set interval of one day 
			timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
			start_timer();
			Console.Read();
		}

		static void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Console.WriteLine("hello");
		}

		private static void start_timer()
		{
			timer.Start();
		}

		protected override void OnStop()
		{
			timer.Stop();
		}
	}
}
