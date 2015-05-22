using Microsoft.Win32.TaskScheduler;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using Teltec.Backup.Data.DAO;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.Svc
{
	public partial class Service : ServiceBase
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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

		public Service()
		{
			ServiceName = typeof(Teltec.Backup.Svc.Service).Namespace;
			CanShutdown = true;
		}

		//static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		//{
		//	Console.Write("hello");
		//}

		private string BuildTaskName(Models.ISchedulablePlan plan)
		{
			return plan.ScheduleParamName;
		}

		private Trigger[] BuildTriggers(Models.ISchedulablePlan plan)
		{
			List<Trigger> triggers = new List<Trigger>();
			Models.PlanSchedule schedule = plan.Schedule;
			switch (schedule.ScheduleType)
			{
				case Models.ScheduleTypeEnum.RUN_MANUALLY:
					{
						break;
					}
				case Models.ScheduleTypeEnum.SPECIFIC:
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
				case Models.ScheduleTypeEnum.RECURRING:
					{
						if (!schedule.RecurrencyFrequencyType.HasValue)
							break;
						
						Trigger tr = null;

						switch (schedule.RecurrencyFrequencyType.Value)
						{
							case Models.FrequencyTypeEnum.DAILY:
								{
									tr = Trigger.CreateTrigger(TaskTriggerType.Daily);
									
									if (schedule.IsRecurrencyDailyFrequencySpecific)
									{
										// Repetition - Occurs every day
										tr.Repetition.Interval = TimeSpan.FromDays(1);
									}
									
									break;
								}
							case Models.FrequencyTypeEnum.WEEKLY:
								{
									if (schedule.OccursAtDaysOfWeek == null || schedule.OccursAtDaysOfWeek.Count == 0)
										break;

									tr = Trigger.CreateTrigger(TaskTriggerType.Weekly);

									WeeklyTrigger wt = tr as WeeklyTrigger;

									Models.PlanScheduleDayOfWeek matchDay = null;

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
							case Models.FrequencyTypeEnum.MONTHLY:
								{
									if (!schedule.MonthlyOccurrenceType.HasValue || !schedule.OccursMonthlyAtDayOfWeek.HasValue)
										break;

									tr = Trigger.CreateTrigger(TaskTriggerType.MonthlyDOW);

									MonthlyDOWTrigger mt = tr as MonthlyDOWTrigger;

									switch (schedule.MonthlyOccurrenceType.Value)
									{
										case Models.MonthlyOccurrenceTypeEnum.FIRST:
											mt.WeeksOfMonth = WhichWeek.FirstWeek;
											break;
										case Models.MonthlyOccurrenceTypeEnum.SECOND:
											mt.WeeksOfMonth = WhichWeek.SecondWeek;
											break;
										case Models.MonthlyOccurrenceTypeEnum.THIRD:
											mt.WeeksOfMonth = WhichWeek.ThirdWeek;
											break;
										case Models.MonthlyOccurrenceTypeEnum.FOURTH:
											mt.WeeksOfMonth = WhichWeek.FourthWeek;
											break;
										case Models.MonthlyOccurrenceTypeEnum.PENULTIMATE:
											mt.WeeksOfMonth = WhichWeek.ThirdWeek;
											break;
										case Models.MonthlyOccurrenceTypeEnum.LAST:
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
							case Models.FrequencyTypeEnum.DAY_OF_MONTH:
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
								case Models.TimeUnitEnum.HOURS:
									tr.Repetition.Interval = TimeSpan.FromHours(schedule.RecurrencyTimeInterval.Value);
									break;
								case Models.TimeUnitEnum.MINUTES:
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

		private Task FindScheduledTask(Models.ISchedulablePlan plan)
		{
			return FindScheduledTask(BuildTaskName(plan));
		}

		private bool HasScheduledTask(string taskName)
		{
			return FindScheduledTask(taskName) != null;
		}

		private bool HasScheduledTask(Models.ISchedulablePlan plan)
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

		private void SchedulePlanExecution(Models.ISchedulablePlan plan, bool reschedule = false)
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

				bool isBackup = plan is Models.BackupPlan;
				bool isRestore = plan is Models.RestorePlan;
				if (!isBackup && !isRestore)
					throw new InvalidOperationException("Unhandled plan type");

				// Create an action that will launch the PlanExecutor
				string planType = isBackup ? "backup" : isRestore ? "restore" : string.Empty;
				string executorCwd = GetExecutableDirectoryPath();
				string executorPath = string.Format(@"{0}\{1}", executorCwd, "Teltec.Backup.PlanExecutor.exe");
				string executorArgs = string.Format("-t {0} -p {1}", planType, plan.ScheduleParamId);
				td.Actions.Add(new ExecAction(executorPath, executorArgs, executorCwd));

				// Register the task in the root folder
				ts.RootFolder.RegisterTaskDefinition(taskName, td, TaskCreation.CreateOrUpdate, null, null, TaskLogonType.InteractiveToken, null);
			}
		}

		List<Models.ISchedulablePlan> AllSchedulablePlans = new List<Models.ISchedulablePlan>();

		private void ReloadPlansAndRescheduler()
		{
			AllSchedulablePlans.Clear();

			BackupPlanRepository daoBackupPlans = new BackupPlanRepository();
			RestorePlanRepository daoRestorePlans = new RestorePlanRepository();

			AllSchedulablePlans.AddRange(daoBackupPlans.GetAll());
			AllSchedulablePlans.AddRange(daoRestorePlans.GetAll());

			foreach (var plan in AllSchedulablePlans)
			{
				SchedulePlanExecution(plan, true);
			}
		}

		//static System.Timers.Timer timer;

		//private static void start_timer()
		//{
		//	timer.Start();
		//}

		protected override void OnStart(string[] args)
		{
			Info("Service is starting...");
			//timer = new System.Timers.Timer();
			//timer.Interval = 2500; //1000 * 60 * 60 * 24; // Set interval of one day 
			//timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
			//start_timer();
			Info("Service was started.");
			ReloadPlansAndRescheduler();
		}

		protected override void OnStop()
		{
			Info("Service is stopping...");
			//timer.Stop();
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

		#region Utils

		private string GetExecutableDirectoryPath()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		#endregion

		#region Logging

		protected void Log(System.Diagnostics.EventLogEntryType type, string message)
		{
			if (EventLog != null)
				EventLog.WriteEntry(message, type);

			switch (type)
			{
				case System.Diagnostics.EventLogEntryType.Error:
					logger.Error(message);
					break;
				case System.Diagnostics.EventLogEntryType.Warning:
					logger.Warn(message);
					break;
				case System.Diagnostics.EventLogEntryType.Information:
					logger.Info(message);
					break;
			}
		}

		protected void Log(System.Diagnostics.EventLogEntryType type, string format, params object[] args)
		{
			string message = string.Format(format, args);
			Log(type, message);
		}

		protected void Warn(string message)
		{
			Log(System.Diagnostics.EventLogEntryType.Warning, message);
		}

		protected void Warn(string format, params object[] args)
		{
			Log(System.Diagnostics.EventLogEntryType.Warning, format, args);
		}

		protected void Error(string message)
		{
			Log(System.Diagnostics.EventLogEntryType.Error, message);
		}

		protected void Error(string format, params object[] args)
		{
			Log(System.Diagnostics.EventLogEntryType.Error, format, args);
		}

		protected void Info(string message)
		{
			Log(System.Diagnostics.EventLogEntryType.Information, message);
		}

		protected void Info(string format, params object[] args)
		{
			Log(System.Diagnostics.EventLogEntryType.Information, format, args);
		}

		#endregion
	}
}
