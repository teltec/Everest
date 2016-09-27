/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Text;

namespace Teltec.Common.Utils
{
	public static class StringUtils
	{
		public static string NormalizeUsingPreferredForm(this string value, NormalizationForm form = NormalizationForm.FormKC)
		{
			return value != null ? (value.IsNormalized(form) ? value : value.Normalize(form)) : null;
		}
	}
}
