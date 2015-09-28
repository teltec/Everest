using System;
using System.Linq.Expressions;

namespace Teltec.Common.Extensions
{
    public static class ObjectExtensions
    {
        public static string GetPropertyName<TSource, TField>(this Object obj, Expression<Func<TSource, TField>> Field)
        {
            return (Field.Body as MemberExpression ?? ((UnaryExpression)Field.Body).Operand as MemberExpression).Member.Name;
        }
    }
}
