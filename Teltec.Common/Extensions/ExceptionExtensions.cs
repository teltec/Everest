/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace Teltec.Common.Extensions
{
	public static class ExceptionExtensions
	{
		public static bool IsCancellation(this Exception ex)
		{
			if (ex.GetType().IsSameOrSubclass(typeof(OperationCanceledException)))
			{
				return true;
			}
			else if (ex.GetType().IsSameOrSubclass(typeof(AggregateException)))
			{
				bool hasCancellation = false;
				IReadOnlyCollection<Exception> flattennedExceptions = (ex as AggregateException).Flatten().InnerExceptions;
				foreach (Exception innerEx in flattennedExceptions)
				{
					if (innerEx.GetType().IsSameOrSubclass(typeof(OperationCanceledException)))
					{
						hasCancellation = true;
						break;
					}
				}

				return hasCancellation;
			}

			return false;
		}
	}
}
