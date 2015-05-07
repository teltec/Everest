using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;

namespace Teltec.Backup.Svc
{
	public partial class Service : ServiceBase
	{
		#region Trap application termination

		/// <summary>
		///  Event set when the process is terminated.
		/// </summary>
		static readonly ManualResetEvent TerminationRequestedEvent = new ManualResetEvent(false);

		/// <summary>
		/// Event set when the process terminates.
		/// </summary>
		static readonly ManualResetEvent TerminationCompletedEvent = new ManualResetEvent(false);

		static Unmanaged.HandlerRoutine Handler;

		static bool OnConsoleEvent(Unmanaged.CtrlTypes reason)
		{
			Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

			// Signal termination
			TerminationRequestedEvent.Set();

			// Wait for cleanup
			TerminationCompletedEvent.WaitOne();

			// Shutdown right away so there are no lingering threads
			Environment.Exit(1);

			// Don't run other handlers, just exit.
			return true;
		}

		static void CatchSpecialConsoleEvents()
		{
			// NOTE: Should NOT use `Console.CancelKeyPress` because it does NOT detect some events: window closing, shutdown, etc.
			
			// Handle special events like: Ctrl+C, window close, kill, shutdown, etc.
			Handler += new Unmanaged.HandlerRoutine(OnConsoleEvent);

			Unmanaged.SetConsoleCtrlHandler(Handler, true);
		}

		#endregion
		
		#region CLI options

		static bool OptionRunAsCLI = false;

		#endregion

		static void Main(string[] args)
		{
			//
			// Parse arguments
			//
			if (args.Contains("-cli"))
			{
				OptionRunAsCLI = true;
			}

			if (OptionRunAsCLI)
			{
				CatchSpecialConsoleEvents();

				Service instance = new Service();
				instance.OnStart(args);

				// Sleep until termination
				TerminationRequestedEvent.WaitOne();

				// Do any cleanups here...
				instance.OnStop();

				// Set this to terminate immediately (if not set, the OS will eventually kill the process)
				TerminationCompletedEvent.Set();
			}
			else
			{
				System.ServiceProcess.ServiceBase.Run(new Service());
			}
		}

		#region Service implementation

		static System.Timers.Timer timer;

		public Service()
		{
			ServiceName = typeof(Teltec.Backup.Svc.Service).Namespace;
			CanShutdown = true;
		}

		static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Console.WriteLine("hello");
		}

		private string BuildTaskName(Teltec.Backup.App.Models.BackupPlan plan)
		{
			string name = string.Format("BackupPlan#{0}", plan.Id.Value);
			return name;
		}

		private string BuildTaskName(Teltec.Backup.App.Models.RestorePlan plan)
		{
			string name = string.Format("RestorePlan#{0}", plan.Id.Value);
			return name;
		}

		private Trigger[] BuildTriggers(Teltec.Backup.App.Models.BackupPlan plan)
		{
			List<Trigger> triggers = new List<Trigger>();
			Teltec.Backup.App.Models.PlanSchedule schedule = plan.Schedule;
			switch (schedule.ScheduleType)
			{
				case App.Models.ScheduleTypeEnum.RUN_MANUALLY:
					{
						break;
					}
				case App.Models.ScheduleTypeEnum.SPECIFIC:
					{
						DateTime? optional = schedule.OccursSpecificallyAt;
						if (!optional.HasValue)
							break;

						DateTime whenToStart = optional.Value;

						Trigger tr = Trigger.CreateTrigger(TaskTriggerType.Custom);
						
						// When to start?
						tr.StartBoundary = whenToStart;
						
						triggers.Add(tr);
						break;
					}
				case App.Models.ScheduleTypeEnum.RECURRING:
					{
						if (!schedule.RecurrencyFrequencyType.HasValue)
							break;
						
						Trigger tr = null;

						switch (schedule.RecurrencyFrequencyType.Value)
						{
							case App.Models.FrequencyTypeEnum.DAILY:
								{
									tr = Trigger.CreateTrigger(TaskTriggerType.Daily);
									
									if (schedule.IsRecurrencyDailyFrequencySpecific)
									{
										// Repetition - Occurs every day
										tr.Repetition.Interval = TimeSpan.FromDays(1);
									}
									
									break;
								}
							case App.Models.FrequencyTypeEnum.WEEKLY:
								{
									if (schedule.OccursAtDaysOfWeek == null || schedule.OccursAtDaysOfWeek.Count == 0)
										break;

									tr = Trigger.CreateTrigger(TaskTriggerType.Weekly);

									WeeklyTrigger wt = tr as WeeklyTrigger;

									App.Models.PlanScheduleDayOfWeek matchDay = null;

									matchDay = schedule.OccursAtDaysOfWeek.First(p => p.DayOfWeek == DayOfWeek.Monday);
									if (matchDay != null)
										wt.DaysOfWeek |= DaysOfTheWeek.Monday;

									matchDay = schedule.OccursAtDaysOfWeek.First(p => p.DayOfWeek == DayOfWeek.Tuesday);
									if (matchDay != null)
										wt.DaysOfWeek |= DaysOfTheWeek.Tuesday;

									matchDay = schedule.OccursAtDaysOfWeek.First(p => p.DayOfWeek == DayOfWeek.Wednesday);
									if (matchDay != null)
										wt.DaysOfWeek |= DaysOfTheWeek.Wednesday;

									matchDay = schedule.OccursAtDaysOfWeek.First(p => p.DayOfWeek == DayOfWeek.Thursday);
									if (matchDay != null)
										wt.DaysOfWeek |= DaysOfTheWeek.Thursday;

									matchDay = schedule.OccursAtDaysOfWeek.First(p => p.DayOfWeek == DayOfWeek.Friday);
									if (matchDay != null)
										wt.DaysOfWeek |= DaysOfTheWeek.Friday;

									matchDay = schedule.OccursAtDaysOfWeek.First(p => p.DayOfWeek == DayOfWeek.Saturday);
									if (matchDay != null)
										wt.DaysOfWeek |= DaysOfTheWeek.Saturday;

									matchDay = schedule.OccursAtDaysOfWeek.First(p => p.DayOfWeek == DayOfWeek.Sunday);
									if (matchDay != null)
										wt.DaysOfWeek |= DaysOfTheWeek.Sunday;
									
									break;
								}
							case App.Models.FrequencyTypeEnum.MONTHLY:
								{
									if (!schedule.MonthlyOccurrenceType.HasValue || !schedule.OccursMonthlyAtDayOfWeek.HasValue)
										break;

									tr = Trigger.CreateTrigger(TaskTriggerType.MonthlyDOW);

									MonthlyDOWTrigger mt = tr as MonthlyDOWTrigger;

									switch (schedule.MonthlyOccurrenceType.Value)
									{
										case App.Models.MonthlyOccurrenceTypeEnum.FIRST:
											mt.WeeksOfMonth = WhichWeek.FirstWeek;
											break;
										case App.Models.MonthlyOccurrenceTypeEnum.SECOND:
											mt.WeeksOfMonth = WhichWeek.SecondWeek;
											break;
										case App.Models.MonthlyOccurrenceTypeEnum.THIRD:
											mt.WeeksOfMonth = WhichWeek.ThirdWeek;
											break;
										case App.Models.MonthlyOccurrenceTypeEnum.FOURTH:
											mt.WeeksOfMonth = WhichWeek.FourthWeek;
											break;
										case App.Models.MonthlyOccurrenceTypeEnum.PENULTIMATE:
											mt.WeeksOfMonth = WhichWeek.ThirdWeek;
											break;
										case App.Models.MonthlyOccurrenceTypeEnum.LAST:
											mt.WeeksOfMonth = WhichWeek.LastWeek;
											break;
									}

									switch (schedule.OccursMonthlyAtDayOfWeek.Value)
									{
										case DayOfWeek.Monday:
											mt.DaysOfWeek = DaysOfTheWeek.Monday;
											break;
										case DayOfWeek.Tuesday:
											mt.DaysOfWeek = DaysOfTheWeek.Tuesday;
											break;
										case DayOfWeek.Wednesday:
											mt.DaysOfWeek = DaysOfTheWeek.Wednesday;
											break;
										case DayOfWeek.Thursday:
											mt.DaysOfWeek = DaysOfTheWeek.Thursday;
											break;
										case DayOfWeek.Friday:
											mt.DaysOfWeek = DaysOfTheWeek.Friday;
											break;
										case DayOfWeek.Saturday:
											mt.DaysOfWeek = DaysOfTheWeek.Saturday;
											break;
										case DayOfWeek.Sunday:
											mt.DaysOfWeek = DaysOfTheWeek.Sunday;
											break;
									}

									break;
								}
							case App.Models.FrequencyTypeEnum.DAY_OF_MONTH:
								{
									if (!schedule.OccursAtDayOfMonth.HasValue)
										break;

									tr = Trigger.CreateTrigger(TaskTriggerType.Monthly);

									MonthlyTrigger mt = tr as MonthlyTrigger;

									//
									// TODO: What happens if the specified day is >=29 and we are in February?
									//
									mt.DaysOfMonth = new int[] { schedule.OccursAtDayOfMonth.Value };

									break;
								}
						}
						
						if (tr == null)
							break;

						// When to start?
						DateTime now = DateTime.UtcNow;
						if (schedule.IsRecurrencyDailyFrequencySpecific)
						{
							TimeSpan? optional = schedule.RecurrencySpecificallyAtTime;
							if (!optional.HasValue)
								break;

							TimeSpan time = optional.Value;
							tr.StartBoundary = new DateTime(now.Year, now.Month, now.Day, time.Hours, time.Minutes, time.Seconds);
						}
						else
						{
							tr.StartBoundary = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0); // Start of day.
						}

						// Repetition - Occurs every interval
						if (!schedule.IsRecurrencyDailyFrequencySpecific)
						{
							switch (schedule.RecurrencyTimeUnit.Value)
							{
								case App.Models.TimeUnitEnum.HOURS:
									tr.Repetition.Interval = TimeSpan.FromHours(schedule.RecurrencyTimeInterval.Value);
									break;
								case App.Models.TimeUnitEnum.MINUTES:
									tr.Repetition.Interval = TimeSpan.FromMinutes(schedule.RecurrencyTimeInterval.Value);
									break;
							}
						}

						// Window limits
						if (!schedule.IsRecurrencyDailyFrequencySpecific)
						{
							if (schedule.RecurrencyWindowStartsAtTime.HasValue && schedule.RecurrencyWindowEndsAtTime.HasValue)
							{
								tr.Repetition.StopAtDurationEnd = false;

								TimeSpan window = schedule.RecurrencyWindowEndsAtTime.Value - schedule.RecurrencyWindowStartsAtTime.Value;

								tr.Repetition.Duration = window;
								//tr.ExecutionTimeLimit = window;
							}
						}

						triggers.Add(tr);
						break;
					}
			}

			if (triggers.Count == 0)
				Warn("No task was created for {0}", BuildTaskName(plan));

			return triggers.ToArray();
		}

		private Task FindScheduledTask(string taskName)
		{
			using (TaskService ts = new TaskService())
			{
				Task task = ts.FindTask(taskName);
				return task;
			}
		}

		private Task FindScheduledTask(Teltec.Backup.App.Models.BackupPlan plan)
		{
			return FindScheduledTask(BuildTaskName(plan));
		}

		private Task FindScheduledTask(Teltec.Backup.App.Models.RestorePlan plan)
		{
			return FindScheduledTask(BuildTaskName(plan));
		}

		private bool HasScheduledTask(string taskName)
		{
			return FindScheduledTask(taskName) != null;
		}

		private bool HasScheduledTask(Teltec.Backup.App.Models.BackupPlan plan)
		{
			return HasScheduledTask(BuildTaskName(plan));
		}

		private bool HasScheduledTask(Teltec.Backup.App.Models.RestorePlan plan)
		{
			return HasScheduledTask(BuildTaskName(plan));
		}

		// Summary:
		//     Returns whether we have Administrator privileges or not.
		public static bool IsElevated
		{
			get
			{
				return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
			}
		}

		private void ScheduleBackup(Teltec.Backup.App.Models.BackupPlan plan, bool reschedule = false)
		{
			string taskName = BuildTaskName(plan);

			using (TaskService ts = new TaskService())
			{
				// Find if there's already a task for the informed plan.
				Task existingTask = ts.FindTask(taskName);

				if (existingTask != null)
				{
					Info("{0} is already scheduled - {1}", taskName,
						reschedule ? "rescheduling..." : "rescheduling was not requested");

					// If we're not rescheduling, stop now.
					if (!reschedule)
						return;

					// Remove the task we found.
					ts.RootFolder.DeleteTask(taskName);
				}
			}

			Info("Scheduling task {0}", taskName);

			string marker = "--\nScheduleBackup started at " + DateTime.Now + "\n";
			File.WriteAllText(@"C:\TeltecBackup\history.txt", marker);

			// Get the service on the local machine
			using (TaskService ts = new TaskService())
			{
				// Create a new task definition and assign properties
				// This task will require Task Scheduler 2.0 (Windows >= Vista or Server >= 2008) or newer.
				TaskDefinition td = ts.NewTask();

				// Run this task even if the user is NOT logged on.
				if (td.LowestSupportedVersion == TaskCompatibility.V1)
					td.Settings.RunOnlyIfLoggedOn = false;

				// When running this task, use the System user account, if we have elevated privileges.
				if (IsElevated)
					td.Principal.LogonType = TaskLogonType.InteractiveTokenOrPassword;

				//td.Principal.RequiredPrivileges = new TaskPrincipalPrivilege[] {
				//	TaskPrincipalPrivilege.SeBackupPrivilege,
				//	TaskPrincipalPrivilege.SeRestorePrivilege,
				//	TaskPrincipalPrivilege.SeChangeNotifyPrivilege,
				//	TaskPrincipalPrivilege.SeCreateSymbolicLinkPrivilege,
				//	TaskPrincipalPrivilege.SeManageVolumePrivilege,
				//	TaskPrincipalPrivilege.SeCreateSymbolicLinkPrivilege,
				//};

				// Run with highest privileges, if we have elevated privileges.
				if (IsElevated)
					td.Principal.RunLevel = TaskRunLevel.Highest;

				// If the task is not scheduled to run again, delete it after 24 hours -- This seem to require `EndBoundary` to be set.
				//td.Settings.DeleteExpiredTaskAfter = TimeSpan.FromHours(24);

				// Don't allow multipe instances of the task to run simultaneously.
				td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;

				// Only run when a network is available.
				td.Settings.RunOnlyIfNetworkAvailable = true;

				td.RegistrationInfo.Author = string.Format(@"{0}\{1}", Environment.UserDomainName, Environment.UserName);

				string description = string.Format("This task was automatically created by the {0} service", typeof(Teltec.Backup.Svc.Service).Namespace);
				td.RegistrationInfo.Description = description;

				// Create triggers to fire the task when planned.
				td.Triggers.AddRange(BuildTriggers(plan));

				// Create an action that will launch Notepad whenever the trigger fires
				td.Actions.Add(new ExecAction(@"C:\Users\jardel\Desktop\Projects\TeltecBackup\timestamp.bat", plan.Id.Value.ToString(), null));

				// Register the task in the root folder
				ts.RootFolder.RegisterTaskDefinition(taskName, td, TaskCreation.CreateOrUpdate, null, null, TaskLogonType.InteractiveToken, null);
			}
		}

		List<App.Models.BackupPlan> AllBackupPlans = new List<App.Models.BackupPlan>();

		private void ReloadPlansAndRescheduler()
		{
			App.DAO.BackupPlanRepository daoBackupPlans = new App.DAO.BackupPlanRepository();
			AllBackupPlans = daoBackupPlans.GetAll();

			foreach (var plan in AllBackupPlans)
			{
				ScheduleBackup(plan, true);
			}
		}

		private static void start_timer()
		{
			timer.Start();
		}

		protected override void OnStart(string[] args)
		{
			Info("Service is starting...");
			timer = new System.Timers.Timer();
			timer.Interval = 2500; //1000 * 60 * 60 * 24; // Set interval of one day 
			timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
			start_timer();
			Info("Service was started.");
			ReloadPlansAndRescheduler();
		}

		protected override void OnStop()
		{
			Info("Service is stopping...");
			timer.Stop();
			Info("Service was stopped.");
		}

		protected override void OnShutdown()
		{
			Info("Service is shutting down...");
			OnStop();
			Info("Service was shutdown..");
		}

		private void OnRefresh()
		{
			Info("Service is refreshing...");
			ReloadPlansAndRescheduler();
			Info("Service was refreshed..");
		}

		protected override void OnCustomCommand(int command)
		{
			switch (command)
			{
				case 205:
					OnRefresh();
					break;
			}
		}

		#endregion

		#region Logging

		private void Log(EventLogEntryType type, string message)
		{
			EventLog.WriteEntry(message, type);
		}

		private void Log(EventLogEntryType type, string format, params object[] args)
		{
			EventLog.WriteEntry(string.Format(format, args), type);
		}

		private void Info(string message)
		{
			Log(EventLogEntryType.Information, message);
		}

		private void Info(string format, params object[] args)
		{
			Log(EventLogEntryType.Information, format, args);
		}

		private void Error(string message)
		{
			Log(EventLogEntryType.Error, message);
		}

		private void Error(string format, params object[] args)
		{
			Log(EventLogEntryType.Error, format, args);
		}

		private void Warn(string message)
		{
			Log(EventLogEntryType.Warning, message);
		}

		private void Warn(string format, params object[] args)
		{
			Log(EventLogEntryType.Warning, format, args);
		}

		#endregion
	}
}
