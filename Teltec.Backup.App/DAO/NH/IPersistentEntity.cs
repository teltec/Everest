
namespace Teltec.Backup.App.DAO.NH
{
	public interface IPersistentEntity
	{
		void OnSave();
		void OnLoad();
		bool IsSaved { get; }
	}
}
