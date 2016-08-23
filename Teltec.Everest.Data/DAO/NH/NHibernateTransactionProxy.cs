using ImpromptuInterface;
using NHibernate;
using System;
using System.Dynamic;

namespace Teltec.Everest.Data.DAO.NH
{
	//
	// "How to make a simple dynamic proxy in C#" by "albertjan" is licensed under CC BY-SA 3.0
	//
	// Title?   How to make a simple dynamic proxy in C#
	// Author?  albertjan - http://stackoverflow.com/users/117242/albertjan
	// Source?  http://stackoverflow.com/a/8387156/298054
	// License? CC BY-SA 3.0 - https://creativecommons.org/licenses/by-sa/3.0/legalcode
	//
	public class NHibernateTransactionProxy<T> : DynamicObject
	{
		private readonly T _wrappedObject;

		public static T1 Wrap<T1>(T obj) where T1 : class
		{
			if (!typeof(T1).IsInterface)
				throw new ArgumentException("T1 must be an Interface");

			return Impromptu.ActLike<T1>(new NHibernateTransactionProxy<T>(obj));
		}

		//you can make the contructor private so you are forced to use the Wrap method.
		private NHibernateTransactionProxy(T obj)
		{
			_wrappedObject = obj;
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			using (ISession sess = NHibernateHelper.GetSession())
			using (ITransaction tx = sess.BeginTransaction())
			{
				try
				{
					result = _wrappedObject.GetType().GetMethod(binder.Name).Invoke(_wrappedObject, args);
					tx.Commit(); // Flush the session and commit the transaction.
					return true;
				}
				catch (Exception)
				{
					tx.Rollback();
					result = null;
					//return false;
					throw;
				}
			}
		}
	}
}
