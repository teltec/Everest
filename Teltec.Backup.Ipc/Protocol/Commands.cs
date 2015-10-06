using System;
using System.Collections.Generic;

namespace Teltec.Backup.Ipc.Protocol
{
	public static class Commands
	{
		public static readonly Command SRV_ERROR = new Command("ERROR")
			.WithArgument("message", typeof(string));

		public static readonly Command SRV_REGISTER = new Command("REGISTER")
			.WithArgument("clientName", typeof(string))
			.AllowAnonymous();

		public static readonly Command SRV_CONTROL = new Command("CONTROL")
			.WithSub(SRV_CONTROL_PLAN);

		public static readonly Command SRV_CONTROL_PLAN = new Command("PLAN")
			.WithSub(SRV_CONTROL_PLAN_RUN)
			.WithSub(SRV_CONTROL_PLAN_RESUME)
			.WithSub(SRV_CONTROL_PLAN_CANCEL)
			.WithSub(SRV_CONTROL_PLAN_KILL);

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

		public static readonly Command CLI_ERROR = new Command("ERROR")
			.WithArgument("message", typeof(string));

		public static readonly Command CLI_CONTROL = new Command("CONTROL");

		public static readonly Command[] CLIENT_COMMANDS = new Command[]
		{
			CLI_ERROR,
			CLI_CONTROL,
		};
		public static readonly CommandParser ClientParser = new CommandParser(CLIENT_COMMANDS);

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

		public static string RunPlan(string planType, Int32 planId)
		{
			if (!IsValidPlanType(planType))
				throw new ArgumentException("Invalid plan type", "planType");

			BoundCommand bound = new BoundCommand(SRV_CONTROL_PLAN_RUN)
				.BindArgument("planType", planType)
				.BindArgument("planId", planId);

			string result = bound.ToString();
			return result;
		}

		public static string ResumePlan(string planType, Int32 planId)
		{
			if (!IsValidPlanType(planType))
				throw new ArgumentException("Invalid plan type", "planType");

			BoundCommand bound = new BoundCommand(SRV_CONTROL_PLAN_RESUME)
				.BindArgument("planType", planType)
				.BindArgument("planId", planId);

			string result = bound.ToString();
			return result;
		}

		public static string CancelPlan(string planType, Int32 planId)
		{
			if (!IsValidPlanType(planType))
				throw new ArgumentException("Invalid plan type", "planType");

			BoundCommand bound = new BoundCommand(SRV_CONTROL_PLAN_CANCEL)
				.BindArgument("planType", planType)
				.BindArgument("planId", planId);

			string result = bound.ToString();
			return result;
		}

		public static string KillPlan(string planType, Int32 planId)
		{
			if (!IsValidPlanType(planType))
				throw new ArgumentException("Invalid plan type", "planType");

			BoundCommand bound = new BoundCommand(SRV_CONTROL_PLAN_KILL)
				.BindArgument("planType", planType)
				.BindArgument("planId", planId);

			string result = bound.ToString();
			return result;
		}

		public static string ReportError(string message)
		{
			return "ERROR " + message;
		}
	}
}
