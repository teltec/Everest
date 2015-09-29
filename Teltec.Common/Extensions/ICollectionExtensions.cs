using System.Collections.Generic;

namespace Teltec.Common.Extensions
{
	public static class ICollectionExtensions
	{
		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> enumerable)
		{
			foreach (var cur in enumerable)
			{
				collection.Add(cur);
			}
		}
	}
}
