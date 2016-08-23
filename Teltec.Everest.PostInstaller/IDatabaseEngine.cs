
namespace PostInstaller
{
	interface IDatabaseEngine
	{
		bool CreateDatabase();
		bool DropDatabase();
	}
}
