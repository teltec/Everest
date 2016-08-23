using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using System;
using Teltec.Everest.Data.DAO.NH;

namespace Teltec.Everest.Data.DAO.Test
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
