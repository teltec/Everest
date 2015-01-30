using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teltec.Backup.Models;

namespace Teltec.Backup.DAO
{
	public class CommonDAO
	{
		protected DBContextScope _dbContextScope;

		public CommonDAO(DBContextScope scope)
		{
			_dbContextScope = scope;
		}
	}
}
