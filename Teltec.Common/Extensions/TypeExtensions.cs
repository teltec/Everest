using System;

namespace Teltec.Common.Extensions
{
	public static class TypeExtensions
	{
		public static bool IsSameOrSubclass(this Type childType, Type baseType)
		{
			return childType == baseType || childType.IsSubclassOf(baseType);
		}
	}
}
