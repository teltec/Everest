/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq.Expressions;

namespace Teltec.Common.Extensions
{
    public static class ObjectExtensions
    {
		//
		// Summary:
		//   Example of use:
		//     Type type = this.GetType();
		//     type.GetPropertyName((x) => x.FullName));
		//
		public static string GetPropertyName<TSource, TField>(this TSource obj, Expression<Func<TSource, TField>> Field)
		{
			return (Field.Body as MemberExpression ?? ((UnaryExpression)Field.Body).Operand as MemberExpression).Member.Name;
		}

		//
		// Summary:
		//   Example of use:
		//     this.GetPropertyName((Type x) => x.FullName));
		//
        public static string GetPropertyName<TSource, TField>(this Object obj, Expression<Func<TSource, TField>> Field)
        {
            return (Field.Body as MemberExpression ?? ((UnaryExpression)Field.Body).Operand as MemberExpression).Member.Name;
        }
    }
}
