using System;
using System.Diagnostics;
using Teltec.Common.Utils;

namespace Teltec.Backup.Data.Models
{
	public class PlanActionExecuteCommand : PlanAction
	{
		public PlanActionExecuteCommand() { }
		public PlanActionExecuteCommand(PlanTriggerTypeEnum triggerType) : base(triggerType) { }

		public const int CommandMaxLen = 160;
		private string _Command;
		public virtual string Command
		{
			get { return _Command; }
			set { SetField(ref _Command, value); }
		}

		public const int ArgumentsMaxLen = 160;
		private string _Arguments;
		public virtual string Arguments
		{
			get { return _Arguments; }
			set { SetField(ref _Arguments, value); }
		}

		public override PlanActionTypeEnum Type
		{
			get { return PlanActionTypeEnum.EXECUTE_COMMAND; }
		}

		public override string Name
		{
			get { return string.Format("{0} {{ cmd={1}, args={2} }}", this.GetType().FullName, this.Command, this.Arguments); }
		}

		public override bool IsValid()
		{
			// Command is NOT NULL
			if (Command == null)
				return false;

			if (Command.Length > CommandMaxLen)
				return false;

			// Arguments may be NULL
			if (Arguments != null && Arguments.Length > ArgumentsMaxLen)
				return false;

			return true;
		}

		public override int Execute(PlanEventArgs args)
		{
			// IMPORTANT: The following code may throw exceptions, and they MUST NOT be handled here
			//            because we want to report errors to the GUI.
			Process process = ProcessUtils.StartSubProcess(this.Command, this.Arguments, Environment.CurrentDirectory);
			process.WaitForExit();
			return process.ExitCode;
		}
	}
}

