
namespace Teltec.Everest.PlanExecutor.Actions
{
	public abstract class TriggerableAction
	{
		public void BeforeExecute()
		{
		}

		public abstract void Execute();

		public void AfterExecute()
		{
		}
	}
}
