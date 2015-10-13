using System;
using System.Collections.Generic;

namespace Teltec.Backup.Ipc.Protocol
{
	public static class Commands
	{
		public static readonly string GUI_CLIENT_NAME = "gui";

		// ----------------------------------------------------------------------------------------

		public static readonly Command SRV_ERROR = new Command("ERROR")
			.WithArgument("message", typeof(string));

		public static readonly Command SRV_REGISTER = new Command("REGISTER")
			.WithArgument("clientName", typeof(string))
			.AllowAnonymous();

		public static readonly Command SRV_CONTROL = new Command("CONTROL")
			.WithSubCommand(SRV_CONTROL_PLAN);

		public static readonly Command SRV_CONTROL_PLAN = new Command("PLAN")
			.WithSubCommand(SRV_CONTROL_PLAN_RUN)
			.WithSubCommand(SRV_CONTROL_PLAN_RESUME)
			.WithSubCommand(SRV_CONTROL_PLAN_CANCEL)
			.WithSubCommand(SRV_CONTROL_PLAN_KILL);

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

		public static readonly Command SRV_ROUTE = new Command("ROUTE")
			.WithArgument("targetName", typeof(string))
			.WithArgument("message", typeof(string));

		public static readonly Command SRV_BROADCAST = new Command("BROADCAST")
			.WithArgument("message", typeof(string));

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
			.WithArgument("message", typeof(string));

		public static readonly Command EXECUTOR_CONTROL = new Command("CONTROL")
			.WithSubCommand(EXECUTOR_CONTROL_PLAN);

		public static readonly Command EXECUTOR_CONTROL_PLAN = new Command("PLAN")
			.WithSubCommand(EXECUTOR_CONTROL_PLAN_CANCEL);

		public static readonly Command EXECUTOR_CONTROL_PLAN_CANCEL = new Command("CANCEL");

		public static readonly Command[] EXECUTOR_COMMANDS = new Command[]
		{
			EXECUTOR_ERROR,
			EXECUTOR_CONTROL,
		};
		public static readonly CommandParser ExecutorParser = new CommandParser(EXECUTOR_COMMANDS);

		// ----------------------------------------------------------------------------------------

		public enum OperationStatus
		{
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

		public static readonly Command GUI_ERROR = new Command("ERROR")
			.WithArgument("message", typeof(string));

		public static readonly Command GUI_REPORT = new Command("REPORT")
			.WithSubCommand(GUI_REPORT_PLAN);

		public static readonly Command GUI_REPORT_PLAN = new Command("PLAN")
			.WithSubCommand(GUI_REPORT_PLAN_PROGRESS);

		public static readonly Command GUI_REPORT_PLAN_STATUS = new Command("STATUS")
			.WithArgument("planType", typeof(string))
			.WithArgument("planId", typeof(Int32))
			.WithArgument("state", typeof(OperationStatus))
			;

		public static readonly Command GUI_REPORT_PLAN_PROGRESS = new Command("PROGRESS")
			.WithArgument("planType", typeof(string))
			.WithArgument("planId", typeof(Int32))
			//.WithArgument(...)
			;

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

		public static string ReportOperationStatus(string planType, Int32 planId, OperationStatus state)
		{
			if (!IsValidPlanType(planType))
				throw new ArgumentException("Invalid plan type", "planType");

			BoundCommand bound = new BoundCommand(GUI_REPORT_PLAN_STATUS);
			bound.BindArgument<string>("planType", planType);
			bound.BindArgument<Int32>("planId", planId);
			bound.BindArgument<string>("state", state.ToString());

			string result = bound.ToString();
			return result;
		}

		public static string ReportError(string message)
		{
			return "ERROR " + message;
		}

		public static string ReportError(string format, params object[] arguments)
		{
			return string.Format("ERROR " + format, arguments);
		}

		public static string BuildClientName(string planType, Int32 planId)
		{
			if (!IsValidPlanType(planType))
				throw new ArgumentException("Invalid plan type", "planType");

			return string.Format("executor:{0}:{1}", planType.ToUpper(), planId);
		}
	}
}
