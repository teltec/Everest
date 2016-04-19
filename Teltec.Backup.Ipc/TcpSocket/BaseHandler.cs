using System.Text;

namespace Teltec.Backup.Ipc.TcpSocket
{
	public abstract class BaseHandler
	{
		protected string BytesToString(byte[] data)
		{
			return Encoding.UTF8.GetString(data);
		}

		protected byte[] StringToBytes(string message)
		{
			return Encoding.UTF8.GetBytes(message + "\n");
		}
	}
}
