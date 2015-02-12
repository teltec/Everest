using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teltec.Backup.Models
{
	public enum EStorageAccountType
	{
		Unknown = 0,
		AmazonS3 = 1,
		FileSystem = 2,
	};
}
