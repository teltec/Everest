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
		protected internal DbContextScope _dbContextScope;

		public CommonDAO(DbContextScope scope)
		{
			_dbContextScope = scope;
		}
	}
}
