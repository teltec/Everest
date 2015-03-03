using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teltec.Common.Extensions
{
	public static class IEnumerableExtensions
	{
		/// <summary>
		/// Concatenates all the elements of an array using the specified delimiter between each element.
		/// 
		/// Arguments:
		/// enumerable: The enumerable itself.
		/// property  : Function to resolve the string property to be used as element.
		/// emptyStr  : String to be returned in case the collection is empty.
		/// delimiter : Separator string to be appended between the elements.
		/// maxLength : Negative value indicates no limit. A positive value limits 
		/// trail     : String to be appended after maxLength truncation.
		/// </summary>
		public static string AsDelimitedString<T>(
			this IEnumerable<T> enumerable,
			Func<T, string> property,
			string emptyStr = "Empty",
			string delimiter = ", ",
			int maxLength = -1,
			string trail = "...")
		{
			int count = Enumerable.Count<T>(enumerable);
			if (count > 0)
			{
				string result = string.Join(delimiter, enumerable.Select(property));
				//string result = Enumerable.Aggregate<T, string>(
				//	enumerable, "", (accum, next) => accum += property(next) + delimiter);
				if (maxLength > 0 && result.Length > maxLength)
					result = result.Substring(0, maxLength - trail.Length) + trail;
				return result;
			}
			else
			{
				return emptyStr;
			}
		}
	}
}
