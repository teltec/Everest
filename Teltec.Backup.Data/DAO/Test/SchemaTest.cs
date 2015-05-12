using System;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using Teltec.Backup.Data.DAO.NH;

namespace Teltec.Backup.Data.DAO.Test
{
	[TestFixture]
	public class SchemaTest
	{
		[Test]
		public void CanGenerateSchema()
		{
			var schemaUpdate = new SchemaUpdate(NHibernateHelper.Configuration);
			schemaUpdate.Execute(Console.WriteLine, true);
		}
	}
}
