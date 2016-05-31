using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using Teltec.Common.Extensions;
using Teltec.Storage;

namespace Teltec.Backup.Data.Models
{
	public class PlanEventArgs : EventArgs
	{
		public object Plan;
		public TransferStatus OperationResult;
	}

	public delegate bool PlanEventHandler(object sender, PlanEventArgs e);

	public interface IPlanConfigActions
	{
		IList<PlanAction> Actions { get; }
		IEnumerable<PlanAction> FilterActionsByTriggerType(PlanTriggerTypeEnum type);

		event PlanEventHandler BeforePlanStarts;
		event PlanEventHandler AfterPlanFinishes;

		bool OnBeforePlanStarts(PlanEventArgs e);
		bool OnAfterPlanFinishes(PlanEventArgs e);

		void WireUpActions();
	}

	public class PlanConfig : BaseEntity<Int32?>, IPlanConfigActions
	{
		private Int32? _Id;
		public virtual Int32? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		#region IPlanConfigEvents

		private IList<PlanAction> _Actions = new List<PlanAction>();
		public virtual IList<PlanAction> Actions
		{
			get { return _Actions; }
			set { SetField(ref _Actions, value); }
		}

		public virtual IEnumerable<PlanAction> FilterActionsByTriggerType(PlanTriggerTypeEnum type)
		{
			return Actions.Where(p => p.TriggerType == type);
		}

		public virtual event PlanEventHandler BeforePlanStarts;
		public virtual event PlanEventHandler AfterPlanFinishes;

		public virtual bool OnBeforePlanStarts(PlanEventArgs e)
		{
			PlanEventHandler handler = BeforePlanStarts;
			if (handler != null)
			{
				return handler(this, e);
			}
			return true;
		}

		public virtual bool OnAfterPlanFinishes(PlanEventArgs e)
		{
			PlanEventHandler handler = AfterPlanFinishes;
			if (handler != null)
			{
				return handler(this, e);
			}
			return true;
		}

		public virtual void WireUpActions()
		{
			foreach (PlanAction action in Actions)
			{
				Func<object, PlanEventArgs, bool> executeActionFunc = (object sender, PlanEventArgs e) =>
				{
					action.ShouldExecute = e.OperationResult == TransferStatus.COMPLETED;
					if (!action.ConsiderShouldExecute || (action.ConsiderShouldExecute && action.ShouldExecute))
					{
						int ret = action.Execute(e);
						if (ret != 0 && action.AbortIfExecutionFails)
						{
							Logger logger = LogManager.GetCurrentClassLogger();
							logger.Warn("Action {0} failed with return value {1}.", action.Name, ret);
							return false; // Signal execution failed
						}
					}
					return true;
				};

				switch (action.TriggerType)
				{
					default:
						string message = string.Format("Unhandled {0} value: {1}", typeof(PlanTriggerTypeEnum).FullName, action.TriggerType);
						throw new ArgumentException(message, action.GetPropertyName((x) => x.TriggerType));
					case PlanTriggerTypeEnum.BEFORE_PLAN_STARTS:
						BeforePlanStarts += executeActionFunc.Invoke;
						break;
					case PlanTriggerTypeEnum.AFTER_PLAN_FINISHES:
						AfterPlanFinishes += executeActionFunc.Invoke;
						break;
				}
			}
		}

		#endregion

	}
}
