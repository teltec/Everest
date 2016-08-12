using System;

namespace Teltec.Backup.Data.Models
{
	public enum PlanTriggerTypeEnum
	{
		UNDEFINED = 0,
		BEFORE_PLAN_STARTS = 1,
		AFTER_PLAN_FINISHES = 2,
	}

	public enum PlanActionTypeEnum
	{
		UNDEFINED = 0,
		EXECUTE_COMMAND = 1,
		NOTIFY_VIA_EMAIL = 2,
	}

	public abstract class PlanAction : BaseEntity<Int32?>
	{
		public PlanAction() { }

		public PlanAction(PlanTriggerTypeEnum triggerType)
		{
			_TriggerType = triggerType;
		}

		private Int32? _Id;
		public virtual Int32? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		private PlanConfig _PlanConfig;
		public virtual PlanConfig PlanConfig
		{
			get { return _PlanConfig; }
			set { SetField(ref _PlanConfig, value); }
		}

		public abstract PlanActionTypeEnum Type
		{
			get;
		}

		private PlanTriggerTypeEnum _TriggerType = PlanTriggerTypeEnum.UNDEFINED;
		public virtual PlanTriggerTypeEnum TriggerType
		{
			get { return _TriggerType; }
			set { SetField(ref _TriggerType, value); }
		}

		private bool _IsEnabled;
		public virtual bool IsEnabled
		{
			get { return _IsEnabled; }
			set { SetField(ref _IsEnabled, value); }
		}

		private bool _ShouldExecute;
		public virtual bool ShouldExecute
		{
			get { return _ShouldExecute; }
			set { SetField(ref _ShouldExecute, value); }
		}

		private bool _ConsiderShouldExecute;
		public virtual bool ConsiderShouldExecute
		{
			get { return _ConsiderShouldExecute; }
			set { SetField(ref _ConsiderShouldExecute, value); }
		}

		private bool _AbortIfExecutionFails;
		public virtual bool AbortIfExecutionFails
		{
			get { return _AbortIfExecutionFails; }
			set { SetField(ref _AbortIfExecutionFails, value); }
		}

		public virtual string Name
		{
			get { return string.Format("{0}", this.GetType().FullName); }
		}

		public abstract bool IsValid();

		public abstract int Execute(PlanEventArgs args);
	}
}

