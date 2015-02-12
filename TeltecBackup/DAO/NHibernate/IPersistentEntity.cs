
namespace Teltec.Backup.DAO.NHibernate
{
	public interface IPersistentEntity
	{
		void OnSave();
		void OnLoad();
		bool IsSaved { get; }
	}
}
