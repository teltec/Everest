
namespace Teltec.Everest.Data.DAO.NH
{
	public interface IPersistentEntity
	{
		void OnSave();
		void OnLoad();
		bool IsSaved { get; }
	}
}
