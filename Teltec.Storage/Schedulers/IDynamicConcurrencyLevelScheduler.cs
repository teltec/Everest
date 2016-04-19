
namespace System.Threading.Tasks.Schedulers
{
	public interface IDynamicConcurrencyLevelScheduler
	{
		void RemovePendingTasks();
		void UpdateMaximumConcurrencyLevel(int value);
	}
}
