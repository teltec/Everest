using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teltec.Everest.Ipc.Protocol
{
	public static class Commands
	{
		public const string IPC_DEFAULT_HOST = "127.0.0.1";
		public const int IPC_DEFAULT_PORT = 35832; // (T)ELTEC translated to keypad, without the T :-)
		public const string IPC_DEFAULT_GUI_CLIENT_NAME = "gui";

		public enum ErrorCode
		{
			INVALID_CMD = 1,
			NOT_AUTHORIZED = 2,
			NAME_ALREADY_IN_USE = 3,
			INVALID_ROUTE_MSG = 4,
			UNKNOWN_TARGET = 5,
		}

		// ----------------------------------------------------------------------------------------

		public static readonly Command SRV_ERROR = new Command("ERROR")
			.WithArgument("errorCode", typeof(int), false)
			.WithArgument("message", typeof(string), true);

		public static readonly Command SRV_REGISTER = new Command("REGISTER")
			.WithArgument("clientName", typeof(string))
			.AllowAnonymous();

		public static readonly Command SRV_CONTROL_PLAN_QUERY = new Command("QUERY")
			.WithArgument("planType", typeof(string))
			.WithArgument("planId", typeof(Int32));

		public static readonly Command SRV_CONTROL_PLAN_RUN = new Command("RUN")
			.WithArgument("planType", typeof(string))
			.WithArgument("planId", typeof(Int32));

		public static readonly Command SRV_CONTROL_PLAN_RESUME = new Command("RESUME")
			.WithArgument("planType", typeof(string))
			.WithArgument("planId", typeof(Int32));

		public static readonly Command SRV_CONTROL_PLAN_CANCEL = new Command("CANCEL")
			.WithArgument("planType", typeof(string))
			.WithArgument("planId", typeof(Int32));

		public static readonly Command SRV_CONTROL_PLAN_KILL = new Command("KILL")
			.WithArgument("planType", typeof(string))
			.WithArgument("planId", typeof(Int32));

		public static readonly Command SRV_CONTROL_PLAN = new Command("PLAN")
			.WithSubCommand(SRV_CONTROL_PLAN_QUERY)
			.WithSubCommand(SRV_CONTROL_PLAN_RUN)
			.WithSubCommand(SRV_CONTROL_PLAN_RESUME)
			.WithSubCommand(SRV_CONTROL_PLAN_CANCEL)
			.WithSubCommand(SRV_CONTROL_PLAN_KILL);

		public static readonly Command SRV_CONTROL = new Command("CONTROL")
			.WithSubCommand(SRV_CONTROL_PLAN);

		public static readonly Command SRV_ROUTE = new Command("ROUTE")
			.WithArgument("targetName", typeof(string))
			.WithArgument("message", typeof(string), true);

		public static readonly Command SRV_BROADCAST = new Command("BROADCAST")
			.WithArgument("message", typeof(string), true);

		public static readonly Command[] SERVER_COMMANDS = new Command[]
		{
			SRV_ERROR,
			SRV_REGISTER,
			SRV_CONTROL,
			SRV_ROUTE,
			SRV_BROADCAST,
		};
		public static readonly CommandParser ServerParser = new CommandParser(SERVER_COMMANDS);

		// ----------------------------------------------------------------------------------------

		public static readonly Command EXECUTOR_ERROR = new Command("ERROR")
			.WithArgument("errorCode", typeof(int), false)
			.WithArgument("message", typeof(string), true);

		public static readonly Command EXECUTOR_CONTROL_PLAN_CANCEL = new Command("CANCEL");

		public static readonly Command EXECUTOR_CONTROL_PLAN = new Command("PLAN")
			.WithSubCommand(EXECUTOR_CONTROL_PLAN_CANCEL);

		public static readonly Command EXECUTOR_CONTROL = new Command("CONTROL")
			.WithSubCommand(EXECUTOR_CONTROL_PLAN);

		public static readonly Command[] EXECUTOR_COMMANDS = new Command[]
		{
			EXECUTOR_ERROR,
			EXECUTOR_CONTROL,
		};
		public static readonly CommandParser ExecutorParser = new CommandParser(EXECUTOR_COMMANDS);

		// ----------------------------------------------------------------------------------------

		public enum OperationStatus
		{
			NOT_RUNNING,
			INTERRUPTED,
			STARTED,
			RESUMED,
			SCANNING_FILES_STARTED,
			SCANNING_FILES_FINISHED,
			PROCESSING_FILES_STARTED,
			PROCESSING_FILES_FINISHED,
			UPDATED,
			FINISHED,
			FAILED,
			CANCELED,
		}

		public static bool IsEnded(this OperationStatus status)
		{
			return status == OperationStatus.NOT_RUNNING
				|| status == OperationStatus.INTERRUPTED
				|| status == OperationStatus.FINISHED
				|| status == OperationStatus.FAILED
				|| status == OperationStatus.CANCELED;
		}

		public class GuiReportPlanStatus : ComplexArgument
		{
			public OperationStatus Status;
			public DateTime? StartedAt;
			public DateTime? FinishedAt;
			public DateTime? LastRunAt;
			public DateTime? LastSuccessfulRunAt;
			//public string ScheduleType;
			public string Sources;
		}

		public class GuiReportPlanProgress : ComplexArgument
		{
			public int Total;
			public int Completed;
			public long BytesTotal;
			public long BytesCompleted;
		}

		public static readonly Command GUI_ERROR = new Command("ERROR")
			.WithArgument("errorCode", typeof(int), false)
			.WithArgument("message", typeof(string), true);

		public static readonly Command GUI_REPORT_PLAN_STATUS = new Command("STATUS")
			.WithArgument("planType", typeof(string))
			.WithArgument("planId", typeof(Int32))
			.WithArgument("report", typeof(GuiReportPlanStatus), true)
			;

		public static readonly Command GUI_REPORT_PLAN_PROGRESS = new Command("PROGRESS")
			.WithArgument("planType", typeof(string))
			.WithArgument("planId", typeof(Int32))
			.WithArgument("progress", typeof(GuiReportPlanProgress), true)
			;

		public static readonly Command GUI_REPORT_PLAN = new Command("PLAN")
			.WithSubCommand(GUI_REPORT_PLAN_STATUS)
			.WithSubCommand(GUI_REPORT_PLAN_PROGRESS);

		public static readonly Command GUI_REPORT = new Command("REPORT")
			.WithSubCommand(GUI_REPORT_PLAN);

		public static readonly Command[] GUI_COMMANDS = new Command[]
		{
			GUI_ERROR,
			GUI_REPORT,
		};
		public static readonly CommandParser GuiParser = new CommandParser(GUI_COMMANDS);

		// ----------------------------------------------------------------------------------------

		public static readonly int REGISTER_CLIENT_NAME_MAXLEN = 32;

		public static string Register(string name)
		{
			BoundCommand bound = new BoundCommand(SRV_REGISTER)
				.BindArgument("clientName", name);
			string result = bound.ToString();
			return result;
		}

		public static bool IsValidPlanType(string planType)
		{
			bool isBackup = planType.Equals("backup", StringComparison.OrdinalIgnoreCase);
			bool isRestore = planType.Equals("restore", StringComparison.OrdinalIgnoreCase);
			return isBackup || isRestore;
		}

		public static string ServerQueryPlan(string planType, Int32 planId)
		{
			if (!IsValidPlanType(planType))
				throw new ArgumentException("Invalid plan type", "planType");

			BoundCommand bound = new BoundCommand(SRV_CONTROL_PLAN_QUERY)
				.BindArgument("planType", planType)
				.BindArgument("planId", planId);

			string result = bound.ToString();
			return result;
		}

		public static string ServerRunPlan(string planType, Int32 planId)
		{
			if (!IsValidPlanType(planType))
				throw new ArgumentException("Invalid plan type", "planType");

			BoundCommand bound = new BoundCommand(SRV_CONTROL_PLAN_RUN)
				.BindArgument("planType", planType)
				.BindArgument("planId", planId);

			string result = bound.ToString();
			return result;
		}

		public static string ServerResumePlan(string planType, Int32 planId)
		{
			if (!IsValidPlanType(planType))
				throw new ArgumentException("Invalid plan type", "planType");

			BoundCommand bound = new BoundCommand(SRV_CONTROL_PLAN_RESUME)
				.BindArgument("planType", planType)
				.BindArgument("planId", planId);

			string result = bound.ToString();
			return result;
		}

		public static string ServerCancelPlan(string planType, Int32 planId)
		{
			if (!IsValidPlanType(planType))
				throw new ArgumentException("Invalid plan type", "planType");

			BoundCommand bound = new BoundCommand(SRV_CONTROL_PLAN_CANCEL)
				.BindArgument("planType", planType)
				.BindArgument("planId", planId);

			string result = bound.ToString();
			return result;
		}

		public static string ServerKillPlan(string planType, Int32 planId)
		{
			if (!IsValidPlanType(planType))
				throw new ArgumentException("Invalid plan type", "planType");

			BoundCommand bound = new BoundCommand(SRV_CONTROL_PLAN_KILL)
				.BindArgument("planType", planType)
				.BindArgument("planId", planId);

			string result = bound.ToString();
			return result;
		}

		public static string ExecutorCancelPlan()
		{
			BoundCommand bound = new BoundCommand(EXECUTOR_CONTROL_PLAN_CANCEL);

			string result = bound.ToString();
			return result;
		}

		public static string GuiReportOperationStatus(string planType, Int32 planId, GuiReportPlanStatus report)
		{
			if (!IsValidPlanType(planType))
				throw new ArgumentException("Invalid plan type", "planType");

			BoundCommand bound = new BoundCommand(GUI_REPORT_PLAN_STATUS);
			bound.BindArgument<string>("planType", planType);
			bound.BindArgument<Int32>("planId", planId);
#if true
			bound.BindArgument<GuiReportPlanStatus>("report", report);
#else
			bound.BindArgument<string>("status", status.ToString());

			string startedAtStr = startedAt.HasValue
				? startedAt.Value.ToLocalTime().ToString("o") // Convert to ISO (doesn't contain whitespace)
				: null;
			string lastRunAtStr = lastRunAt.HasValue
				? lastRunAt.Value.ToLocalTime().ToString("o") // Convert to ISO (doesn't contain whitespace)
				: "Never";
			string lastSuccessfulRunAtStr = lastSuccessfulRunAt.HasValue
				? lastSuccessfulRunAt.Value.ToLocalTime().ToString("o") // Convert to ISO (doesn't contain whitespace)
				: "Never";

			string sourcesStr = EncodeString(sources);

			bound.BindArgument<string>("report", startedAtStr);
			bound.BindArgument<string>("lastRunAt", lastRunAtStr);
			bound.BindArgument<string>("lastSuccessfulRunAt", lastSuccessfulRunAtStr);
			bound.BindArgument<string>("scheduleType", scheduleType);
			bound.BindArgument<string>("sources", sourcesStr);
#endif

			string result = bound.ToString();
			return result;
		}

		public static string GuiReportOperationProgress(string planType, Int32 planId, GuiReportPlanProgress progress)
		{
			if (!IsValidPlanType(planType))
				throw new ArgumentException("Invalid plan type", "planType");

			BoundCommand bound = new BoundCommand(GUI_REPORT_PLAN_PROGRESS);
			bound.BindArgument<string>("planType", planType);
			bound.BindArgument<Int32>("planId", planId);
			bound.BindArgument<GuiReportPlanProgress>("progress", progress);

			string result = bound.ToString();
			return result;
		}

		public static string WrapToRoute(string targetName, string message)
		{
			BoundCommand bound = new BoundCommand(SRV_ROUTE);
			bound.BindArgument<string>("targetName", targetName);
			bound.BindArgument<string>("message", message);
			string result = bound.ToString();
			return result;
		}

		public static string ReportError(int errorCode, string message)
		{
			return "ERROR " + errorCode + " " + message;
		}

		public static string ReportError(int errorCode, string format, params object[] arguments)
		{
			return string.Format("ERROR " + errorCode + " " + format, arguments);
		}

		public static string BuildClientName(string planType, Int32 planId)
		{
			if (!IsValidPlanType(planType))
				throw new ArgumentException("Invalid plan type", "planType");

			return string.Format("executor:{0}:{1}", planType.ToUpper(), planId);
		}
	}
}
