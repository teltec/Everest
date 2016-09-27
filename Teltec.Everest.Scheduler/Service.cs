/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.Win32.TaskScheduler;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using Teltec.Everest.Data.DAO;
using Teltec.Everest.Ipc.Protocol;
using Teltec.Everest.Ipc.TcpSocket;
using Teltec.Everest.Logging;
using Teltec.Common;
using Teltec.Common.Extensions;
using Teltec.Common.Threading;
using Teltec.Common.Utils;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.Scheduler
{
	public enum PlanTypeEnum
	{
		BACKUP = 0,
		RESTORE = 1,
	}

	public partial class Service : ServiceBase
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private ISynchronizeInvoke SynchronizingObject = new MockSynchronizeInvoke();
		private ServerHandler Handler;

		private const int RefreshCommand = 205;

		#region Main

		static void Main(string[] args)
		{
			try
			{
				UnsafeMain(args);
			}
			catch (Exception ex)
			{
				if (Environment.UserInteractive)
				{
					string message = string.Format(
						"Caught a fatal exception ({0}). Check the log file for more details.",
						ex.Message);
					//if (Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero)
					//	MessageBox.Show(message);
				}
				logger.Log(LogLevel.Fatal, ex, "Caught a fatal exception");
			}
		}

		static void UnsafeMain(string[] args)
		{
			LoggingHelper.ChangeFilenamePostfix("scheduler");

			if (System.Environment.UserInteractive)
			{
				if (args.Length > 0)
				{
					switch (args[0])
					{
						case "-install":
						case "-i":
							ServiceHelper.SelfInstall();
							logger.Info("Service installed");
							ServiceHelper.SelfStart();
							break;
                        case "-r":
                        case "-reinstall":
                            try
                            {
                                ServiceHelper.SelfUninstall();
                                logger.Info("Service uninstalled");
                            }
                            catch (Exception ex)
                            {
                                if (ex.InnerException is Win32Exception)
                                {
                                    Win32Exception win32ex = ex.InnerException as Win32Exception;
                                    if (win32ex.NativeErrorCode != 0x424) // 0x424: ERROR_SERVICE_DOES_NOT_EXIST
                                        throw;
                                }
                                else
                                {
                                    throw;
                                }
                            }
                            
                            ServiceHelper.SelfInstall();
                            logger.Info("Service installed");
                            ServiceHelper.SelfStart();

                            break;
                        case "-uninstall":
						case "-u":
							ServiceHelper.SelfUninstall();
							logger.Info("Service uninstalled");
							break;
					}
				}
				else
				{
					ConsoleAppHelper.CatchSpecialConsoleEvents();

					Service instance = new Service();
					instance.OnStart(args);

					// If initialization failed, then cleanup/OnStop is already done.
					if (instance.ExitCode == 0)
					{
						// Sleep until termination
						ConsoleAppHelper.TerminationRequestedEvent.WaitOne();

						// Do any cleanups here...
						instance.OnStop();

						// Set this to terminate immediately (if not set, the OS will eventually kill the process)
						ConsoleAppHelper.TerminationCompletedEvent.Set();
					}
				}
			}
			else
			{
				ServiceBase.Run(new Service());
			}
		}

		#endregion

		public Service()
		{
			InitializeComponent();

			ServiceName = typeof(Teltec.Everest.Scheduler.Service).Namespace;
			CanShutdown = true;

			Handler = new ServerHandler(SynchronizingObject);
			Handler.OnControlPlanQuery += OnControlPlanQuery;
			Handler.OnControlPlanRun += OnControlPlanRun;
			Handler.OnControlPlanResume += OnControlPlanResume;
			Handler.OnControlPlanCancel += OnControlPlanCancel;
			Handler.OnControlPlanKill += OnControlPlanKill;
		}

		private void InitializeComponent()
		{

		}

		private string BuildTaskName(Models.ISchedulablePlan plan)
		{
			return plan.ScheduleParamName;
		}

		#region Triggers

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

						Trigger tr = Trigger.CreateTrigger(TaskTriggerType.Time);

						// When to start?
						tr.StartBoundary = whenToStart.ToLocalTime();

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

									// IMPORTANT: The default constructed `WeeklyTrigger` sets Sunday.
									wt.DaysOfWeek = 0;

									Models.PlanScheduleDayOfWeek matchDay = null;

									matchDay = schedule.OccursAtDaysOfWeek.SingleOrDefault(p => p.DayOfWeek == DayOfWeek.Monday);
									if (matchDay != null)
										wt.DaysOfWeek |= DaysOfTheWeek.Monday;

									matchDay = schedule.OccursAtDaysOfWeek.SingleOrDefault(p => p.DayOfWeek == DayOfWeek.Tuesday);
									if (matchDay != null)
										wt.DaysOfWeek |= DaysOfTheWeek.Tuesday;

									matchDay = schedule.OccursAtDaysOfWeek.SingleOrDefault(p => p.DayOfWeek == DayOfWeek.Wednesday);
									if (matchDay != null)
										wt.DaysOfWeek |= DaysOfTheWeek.Wednesday;

									matchDay = schedule.OccursAtDaysOfWeek.SingleOrDefault(p => p.DayOfWeek == DayOfWeek.Thursday);
									if (matchDay != null)
										wt.DaysOfWeek |= DaysOfTheWeek.Thursday;

									matchDay = schedule.OccursAtDaysOfWeek.SingleOrDefault(p => p.DayOfWeek == DayOfWeek.Friday);
									if (matchDay != null)
										wt.DaysOfWeek |= DaysOfTheWeek.Friday;

									matchDay = schedule.OccursAtDaysOfWeek.SingleOrDefault(p => p.DayOfWeek == DayOfWeek.Saturday);
									if (matchDay != null)
										wt.DaysOfWeek |= DaysOfTheWeek.Saturday;

									matchDay = schedule.OccursAtDaysOfWeek.SingleOrDefault(p => p.DayOfWeek == DayOfWeek.Sunday);
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

		#endregion

		// Summary:
		//     Returns whether we have Administrator privileges or not.
		public static bool IsElevated
		{
			get
			{
				return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
			}
		}

		#region Scheduling

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

		private void SchedulePlanExecution(Models.ISchedulablePlan plan, bool reschedule = false)
		{
			string taskName = BuildTaskName(plan);

			// Get the service on the local machine
			using (TaskService ts = new TaskService())
			{
				// Find if there's already a task for the informed plan.
				Task existingTask = ts.FindTask(taskName, false);

				if (existingTask != null)
				{
					// Check if the plan changed after the existing task was scheduled.
					// It's important to convert the DateTime's to the same TimeZone before comparing them.
					bool changed = plan.UpdatedAt.ToLocalTime() > existingTask.Definition.RegistrationInfo.Date.ToLocalTime();
					if (!changed)
						return;

					if (plan.IsRunManually)
					{
						Info("{0} is already scheduled - Deleting schedule because it's now Manual.", taskName);

						// Remove the task we found.
						ts.RootFolder.DeleteTask(taskName);
						return;
					}
					else
					{
						Info("{0} is already scheduled - {1}", taskName,
							reschedule ? "rescheduling..." : "rescheduling was not requested");

						// If we're not rescheduling, stop now.
						if (!reschedule)
							return;

						// Do NOT delete the task we found - it will be updated by `RegisterTaskDefinition`.
						//ts.RootFolder.DeleteTask(taskName);
					}
				}
				else
				{
					if (plan.IsRunManually)
					{
						// Do not schedule anything.
						return;
					}
				}

				Info("Scheduling task {0} (plan last changed at {1})", taskName,
					plan.UpdatedAt.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ssK"));

				// If the task doesn't exist yet, create a new task definition and assign properties
				// This task will require Task Scheduler 2.0 (Windows >= Vista or Server >= 2008) or newer.
				TaskDefinition td = existingTask != null
					? existingTask.Definition
					: ts.NewTask();

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

				// We identify the Scheduled task needs an update if this Date is older than `SchedulablePlan.UpdatedAt`.
				td.RegistrationInfo.Date = DateTime.UtcNow;

				string description = string.Format(
					"This task was automatically {0} by the {1} service at {2}",
					existingTask != null ? "updated" : "created",
					typeof(Teltec.Everest.Scheduler.Service).Namespace,
					td.RegistrationInfo.Date.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ssK"));
				td.RegistrationInfo.Description = description;

				// Create triggers to fire the task when planned.
				td.Triggers.Clear();
				td.Triggers.AddRange(BuildTriggers(plan));

				bool isBackup = plan is Models.BackupPlan;
				bool isRestore = plan is Models.RestorePlan;
				if (!isBackup && !isRestore)
					throw new InvalidOperationException("Unhandled plan type");

				// Create an action that will launch the PlanExecutor
				string planType = isBackup
					? PlanTypeEnum.BACKUP.ToString().ToLowerInvariant()
					: isRestore
						? PlanTypeEnum.RESTORE.ToString().ToLowerInvariant()
						: string.Empty;
				PlanExecutorEnv env = BuildPlanExecutorEnv(planType, plan.ScheduleParamId, false);
				td.Actions.Clear();
				td.Actions.Add(new ExecAction(env.Path, env.Arguments, env.Cwd));

				// Register the task in the root folder
				const string username = "SYSTEM";
				const string password = null;
				const TaskLogonType logonType = TaskLogonType.ServiceAccount;
				try
				{
					ts.RootFolder.RegisterTaskDefinition(taskName, td, TaskCreation.CreateOrUpdate, username, password, logonType);
				}
				catch (Exception ex)
				{
					logger.Error("Failed to create/update scheduled task ({0}): {1}", taskName, ex.Message);
				}
			}
		}

		#endregion

		private struct PlanExecutorEnv
		{
			public string Path;
			public string Arguments;
			public string Cwd;
		}

		private void ValidatePlanType(string planType)
		{
			if (!planType.Equals(PlanTypeEnum.BACKUP.ToString().ToLowerInvariant()) && !planType.Equals(PlanTypeEnum.RESTORE.ToString().ToLowerInvariant()))
				throw new ArgumentException("Invalid plan type", "planType");
		}

		private PlanExecutorEnv BuildPlanExecutorEnv(string planType, Int32 planId, bool resume)
		{
			ValidatePlanType(planType);

			string clientName = Commands.BuildClientName(planType, planId);

			PlanExecutorEnv env = new PlanExecutorEnv();
			env.Cwd = GetExecutableDirectoryPath();
			env.Path = Path.Combine(env.Cwd, "Teltec.Everest.PlanExecutor.exe");

			StringBuilder sb = new StringBuilder(255);
			sb.AppendFormat(" --client-name={0}", clientName);
			sb.AppendFormat(" -t {0}", planType);
			sb.AppendFormat(" -p {0}", planId);
			if (resume)
				sb.Append(" --resume");

			env.Arguments = sb.ToString();

			return env;
		}

		#region Remote messages

		private void OnControlPlanQuery(object sender, ServerCommandEventArgs e)
		{
			string planType = e.Command.GetArgumentValue<string>("planType");
			Int32 planId = e.Command.GetArgumentValue<Int32>("planId");

			ValidatePlanType(planType);

			bool isRunning = IsPlanRunning(planType, planId);
			bool needsResume = false;
			bool isFinished = false;

			bool isBackup = planType.Equals(PlanTypeEnum.BACKUP.ToString().ToLowerInvariant());
			bool isRestore = planType.Equals(PlanTypeEnum.RESTORE.ToString().ToLowerInvariant());

			// Report to GUI.
			Commands.GuiReportPlanStatus report = new Commands.GuiReportPlanStatus();

			if (isBackup)
			{
				BackupRepository daoBackup = new BackupRepository();
				Models.Backup latest = daoBackup.GetLatestByPlan(new Models.BackupPlan { Id = planId });

				needsResume = latest != null && latest.NeedsResume();
				isFinished = latest != null && latest.IsFinished();

				if (isRunning)
					report.StartedAt = latest.StartedAt;
				else if (isFinished)
					report.FinishedAt = latest.FinishedAt;
			}
			else if (isRestore)
			{
				RestoreRepository daoRestore = new RestoreRepository();
				Models.Restore latest = daoRestore.GetLatestByPlan(new Models.RestorePlan { Id = planId });

				needsResume = latest != null && latest.NeedsResume();
				isFinished = latest != null && latest.IsFinished();

				if (isRunning)
					report.StartedAt = latest.StartedAt;
				else if (isFinished)
					report.FinishedAt = latest.FinishedAt;
			}

			bool isInterrupted = !isRunning && needsResume;

			Commands.OperationStatus status;
			// The condition order below is important because more than one flag might be true.
			if (isInterrupted)
				status = Commands.OperationStatus.INTERRUPTED;
			else if (needsResume)
				status = Commands.OperationStatus.RESUMED;
			else if (isRunning)
				status = Commands.OperationStatus.STARTED;
			else
				status = Commands.OperationStatus.NOT_RUNNING;

			report.Status = status;

			Handler.Send(e.Context, Commands.GuiReportOperationStatus(planType, planId, report));
		}

		private void OnControlPlanRun(object sender, ServerCommandEventArgs e)
		{
			string planType = e.Command.GetArgumentValue<string>("planType");
			Int32 planId = e.Command.GetArgumentValue<Int32>("planId");

			if (IsPlanRunning(planType, planId))
			{
				string msg = Commands.ReportError(0, "{0} plan #{1} is already running", planType.ToTitleCase(), planId);
				Handler.Send(e.Context, msg);
				return;
			}

			bool isBackup = planType.Equals(PlanTypeEnum.BACKUP.ToString().ToLowerInvariant());
			bool isRestore = planType.Equals(PlanTypeEnum.RESTORE.ToString().ToLowerInvariant());
			const bool isResume = false;

			bool didRun = false;
			if (isBackup)
				didRun = RunBackupPlan(e.Context, planId, isResume);
			else if (isRestore)
				didRun = RunRestorePlan(e.Context, planId, isResume);
		}

		private void OnControlPlanResume(object sender, ServerCommandEventArgs e)
		{
			string planType = e.Command.GetArgumentValue<string>("planType");
			Int32 planId = e.Command.GetArgumentValue<Int32>("planId");

			if (IsPlanRunning(planType, planId))
			{
				string msg = Commands.ReportError(0, "{0} plan #{1} is already running", planType.ToTitleCase(), planId);
				Handler.Send(e.Context, msg);
				return;
			}

			bool isBackup = planType.Equals(PlanTypeEnum.BACKUP.ToString().ToLowerInvariant());
			bool isRestore = planType.Equals(PlanTypeEnum.RESTORE.ToString().ToLowerInvariant());
			const bool isResume = true;

			bool didRun = false;
			if (isBackup)
				didRun = RunBackupPlan(e.Context, planId, isResume);
			else if (isRestore)
				didRun = RunRestorePlan(e.Context, planId, isResume);
		}

		private void OnControlPlanCancel(object sender, ServerCommandEventArgs e)
		{
			string planType = e.Command.GetArgumentValue<string>("planType");
			Int32 planId = e.Command.GetArgumentValue<Int32>("planId");

			if (!IsPlanRunning(planType, planId))
			{
				string msg = Commands.ReportError(0, "{0} plan #{1} is not running", planType.ToTitleCase(), planId);
				Handler.Send(e.Context, msg);
				return;
			}

			// Send to executor
			string executorClientName = Commands.BuildClientName(planType, planId);
			ClientState executor = Handler.GetClientState(executorClientName);
			if (executor == null)
			{
				string msg = Commands.ReportError(0, "Executor for {0} plan #{1} doesn't seem to be running",
					planType.ToTitleCase(), planId);
				Handler.Send(e.Context, msg);
				return;
			}

			Handler.Send(executor.Context, Commands.ExecutorCancelPlan());
		}

		private void KillAllSubProcesses()
		{
			foreach (var entry in RunningBackups)
				entry.Value.Kill();
			RunningBackups.Clear();

			foreach (var entry in RunningRestores)
				entry.Value.Kill();
			RunningRestores.Clear();
		}

		private void OnControlPlanKill(object sender, ServerCommandEventArgs e)
		{
			string planType = e.Command.GetArgumentValue<string>("planType");
			Int32 planId = e.Command.GetArgumentValue<Int32>("planId");

			Process processToBeKilled = null;
			bool isBackup = planType.Equals(PlanTypeEnum.BACKUP.ToString().ToLowerInvariant());
			bool isRestore = planType.Equals(PlanTypeEnum.RESTORE.ToString().ToLowerInvariant());

			if (isBackup)
				RunningBackups.TryGetValue(planId, out processToBeKilled);
			else if (isRestore)
				RunningRestores.TryGetValue(planId, out processToBeKilled);

			if (processToBeKilled == null)
			{
				string msg = Commands.ReportError(0, "{0} plan #{1} is not running", planType.ToTitleCase(), planId);
				Handler.Send(e.Context, msg);
				return;
			}

			processToBeKilled.Kill();

			if (isBackup)
				RunningBackups.Remove(planId);
			else if (isRestore)
				RunningRestores.Remove(planId);
		}

		#endregion

		#region Sub-Process

		private Dictionary<Int32, Process> RunningBackups = new Dictionary<Int32, Process>();
		private Dictionary<Int32, Process> RunningRestores = new Dictionary<Int32, Process>();

		private bool IsPlanRunning(string planType, Int32 planId)
		{
			ValidatePlanType(planType);

			if (planType.Equals(PlanTypeEnum.BACKUP .ToString().ToLowerInvariant()))
				return IsBackupPlanRunning(planId);

			if (planType.Equals(PlanTypeEnum.RESTORE.ToString().ToLowerInvariant()))
				return IsRestorePlanRunning(planId);

			return false;
		}

		private bool IsRestorePlanRunning(Int32 planId)
		{
			return RunningRestores.ContainsKey(planId);
		}

		private bool IsBackupPlanRunning(Int32 planId)
		{
			return RunningBackups.ContainsKey(planId);
		}

		private bool RunRestorePlan(Server.ClientContext context, Int32 planId, bool resume)
		{
			PlanExecutorEnv env = BuildPlanExecutorEnv(PlanTypeEnum.RESTORE.ToString().ToLowerInvariant(), planId, resume);
			EventHandler onExit = delegate(object sender, EventArgs e)
			{
				RunningRestores.Remove(planId);
				//Process process = (Process)sender;
				//if (process.ExitCode != 0)
				//{
				//	Handler.Send(context, Commands.ReportError("FAILED"));
				//}
			};
			try
			{
				Process process = ProcessUtils.StartSubProcess(env.Path, env.Arguments, env.Cwd, onExit, false, false, true);
				RunningRestores.Add(planId, process);
				return true;
			}
			catch (Exception ex)
			{
				Handler.Send(context, Commands.ReportError(0, ex.Message));
				return false;
			}
		}

		private bool RunBackupPlan(Server.ClientContext context, Int32 planId, bool resume)
		{
			PlanExecutorEnv env = BuildPlanExecutorEnv(PlanTypeEnum.BACKUP.ToString().ToLowerInvariant(), planId, resume);
			EventHandler onExit = delegate(object sender, EventArgs e)
				{
					RunningBackups.Remove(planId);
					//Process process = (Process)sender;
					//if (process.ExitCode != 0)
					//{
					//	Handler.Send(context, Commands.ReportError("FAILED"));
					//}
				};
			try
			{
				Process process = ProcessUtils.StartSubProcess(env.Path, env.Arguments, env.Cwd, onExit, false, false, true);
				RunningBackups.Add(planId, process);
				return true;
			}
			catch (Exception ex)
			{
				Handler.Send(context, Commands.ReportError(0, ex.Message));
				return false;
			}
		}

		#endregion

		List<Models.ISchedulablePlan> AllSchedulablePlans = new List<Models.ISchedulablePlan>();

		private void ReloadPlansAndReschedule()
		{
			AllSchedulablePlans.Clear();

			BackupPlanRepository daoBackupPlans = new BackupPlanRepository();
			RestorePlanRepository daoRestorePlans = new RestorePlanRepository();

			AllSchedulablePlans.AddRange(daoBackupPlans.GetAllActive());
			AllSchedulablePlans.AddRange(daoRestorePlans.GetAllActive());

			// TODO(jweyrich): Currently does not DELETE existing tasks for plans that no longer exist.
			// TODO(jweyrich): Currently does not CHECK if an existing plan schedule has been changed.
			foreach (var plan in AllSchedulablePlans)
			{
				SchedulePlanExecution(plan, true);
			}
		}

		static System.Timers.Timer timer;

		private static void start_timer()
		{
			timer.Start();
		}

		static Int64 ExecutionCounter = 0;

		private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			ExecutionCounter++;

			Info("Time to check for changes...");

			try
			{
				ReloadPlansAndReschedule();
			}
			catch (Exception ex)
			{
				if (Environment.UserInteractive)
				{
					string message = string.Format(
						"Caught a fatal exception ({0}). Check the log file for more details.",
						ex.Message);
					//if (Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero)
					//	MessageBox.Show(message);
				}
				logger.Log(LogLevel.Fatal, ex, "Caught a fatal exception");
			}
		}

		#region Service

		protected override void OnStart(string[] args)
		{
			base.OnStart(args);

			// Update the service state to Start Pending.
			//ServiceStatus serviceStatus = new ServiceStatus();
			//serviceStatus.dwServiceType = ServiceInstaller.SERVICE_WIN32_OWN_PROCESS;
			//serviceStatus.dwCurrentState = ServiceState.StartPending;
			//serviceStatus.dwControlsAccepted = 205;
			//serviceStatus.dwCheckPoint++;
			//serviceStatus.dwWaitHint = 15000;
			//ServiceInstaller.SetServiceStatus(this.ServiceHandle, ref serviceStatus);

			Info("Service is starting...");

			timer = new System.Timers.Timer();
			timer.Interval = 1000 * 60 * 5; // Set interval to 5 minutes
			timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);

			// Update the service state to Running.
			//serviceStatus.dwCurrentState = ServiceState.Running;
			//ServiceInstaller.SetServiceStatus(this.ServiceHandle, ref serviceStatus);

			ReloadPlansAndReschedule();

			try
			{
				Handler.Start(Commands.IPC_DEFAULT_HOST, Commands.IPC_DEFAULT_PORT);
			}
			catch (Exception ex)
			{
				Error("Couldn't start the server: {0}", ex.Message);
				base.ExitCode = 1; // Signal the initialization failed.
				base.Stop();
				return;
			}

			Info("Service was started.");

			// Start timer only after the plans were already loaded and rescheduled.
			start_timer();
		}

		protected override void OnStop()
		{
			base.OnStop();

			Info("Service is stopping...");

			if (timer.Enabled)
				timer.Stop();

			KillAllSubProcesses();

			if (Handler != null && Handler.IsRunning)
			{
				Handler.RequestStop();
				Handler.Wait();
			}

			Info("Service was stopped.");
		}

		protected override void OnShutdown()
		{
			base.OnShutdown();

			Info("Service is shutting down...");
			OnStop();
			Info("Service was shutdown.");
		}

		private void OnRefresh()
		{
			Info("Service is refreshing...");
			ReloadPlansAndReschedule();
			Info("Service was refreshed.");
		}

		protected override void OnCustomCommand(int command)
		{
			base.OnCustomCommand(command);

			switch (command)
			{
				case RefreshCommand:
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

		#region Dispose Pattern Implementation

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Handler != null)
				{
					Handler.Dispose();
					Handler = null;
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
