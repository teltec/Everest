using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teltec.Common.Extensions
{
	public static class ICollectionExtension
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
