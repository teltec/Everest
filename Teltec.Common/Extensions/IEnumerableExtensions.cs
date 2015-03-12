using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teltec.Common.Extensions
{
	public static class IEnumerableExtensions
	{
		// Converts `IEnumerable<TSource>` to `LinkedList<TTarget>` using the specified selector for conversion.
		public static LinkedList<TTarget> ToLinkedList<TTarget, TSource>(this IEnumerable<TSource> source, Func<TSource, TTarget> selector)
		{
			LinkedList<TTarget> to = new LinkedList<TTarget>();
			foreach (TSource item in source)
				to.AddLast(selector(item));
			var a = from item in source
					select selector(item);
			return to;
		}

		// Converts `IEnumerable<TSource>` to `IEnumerable<TTarget>` using the specified `convert` action.
		public static IEnumerable<TTarget> Convert<TTarget, TSource>(this IEnumerable<TSource> source, Func<TSource, TTarget> convert)
		{
			var result = from item in source
						 select convert(item);
			return result;
		}

		private static void ValidateCollectionCtorConversion<TTarget, TSource>()
		{
			Type sourceType = typeof(TSource);
			Type targetType = typeof(TTarget);
			var ctor = targetType.GetConstructor(new Type[] { sourceType });
			Assert.IsNotNull(ctor, "type {0} has no constructor that takes an argument of type {1}",
				targetType.FullName, sourceType.FullName);
		}

		// Converts `IEnumerable<TSource>` to `List<TTarget>`.
		// It does not require `TSource` to be compatible with `TTarget`, but
		// requires `TTarget` to have the following constructor signature: `TTarget(TSource)`
		public static List<TTarget> ToListWithCtorConversion<TTarget, TSource>(this IEnumerable<TSource> source)
		{
			IEnumerable<TTarget> converted = Convert(source, p => (TTarget)Activator.CreateInstance(typeof(TTarget), p));
			return new List<TTarget>(converted);

			//ValidateCollectionCtorConversion<TTarget, TSource>();
			//List<TTarget> to = new List<TTarget>(source.Count());
			//foreach (TSource item in source)
			//	to.Add((TTarget)Activator.CreateInstance(typeof(TTarget), item));
			//return to;
		}

		// Converts `IEnumerable<TSource>` to `LinkedList<TTarget>`.
		// It does not require `TSource` to be compatible with `TTarget`, but
		// requires `TTarget` to have the following constructor signature: `TTarget(TSource)`
		public static LinkedList<TTarget> ToLinkedListWithCtorConversion<TTarget, TSource>(this IEnumerable<TSource> source)
		{
			IEnumerable<TTarget> converted = Convert(source, p => (TTarget)Activator.CreateInstance(typeof(TTarget), p));
			return new LinkedList<TTarget>(converted);

			//ValidateCollectionCtorConversion<TTarget, TSource>();
			//LinkedList<TTarget> to = new LinkedList<TTarget>();
			//foreach (TSource item in source)
			//	to.AddLast((TTarget)Activator.CreateInstance(typeof(TTarget), item));
			//return to;
		}

		// Converts `IEnumerable<TSource>` to `Dictionary<TTargetKey, TTargetValue>`.
		// It does not require `TSource` to be compatible with `TTargetValue`, but
		// requires `TTargetValue` to have the following constructor signature: `TTargetValue(TSource)`
		public static Dictionary<TTargetKey, TTargetValue> ToDictionaryWithCtorConversion<TTargetKey, TTargetValue, TSource>(this IEnumerable<TSource> source, Func<TSource, TTargetKey> keySelector)
		{
			ValidateCollectionCtorConversion<TTargetValue, TSource>();
			Dictionary<TTargetKey, TTargetValue> to = new Dictionary<TTargetKey, TTargetValue>(source.Count());
			foreach (TSource item in source)
				to.Add((TTargetKey)keySelector(item), (TTargetValue)Activator.CreateInstance(typeof(TTargetValue), item));
			return to;
		}

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
