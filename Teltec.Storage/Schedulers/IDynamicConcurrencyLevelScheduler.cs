
namespace System.Threading.Tasks.Schedulers
{
	public interface IDynamicConcurrencyLevelScheduler
	{
		void UpdateMaximumConcurrencyLevel(int value);
	}
}
