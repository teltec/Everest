/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

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
