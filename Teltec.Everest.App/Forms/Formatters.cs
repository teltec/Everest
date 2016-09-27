/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows.Forms;

namespace Teltec.Everest.App.Forms
{
	internal static class PlanCommon
	{
		#region Formatters

		public static string FormatTitle(string value)
		{
			ConvertEventArgs e = new ConvertEventArgs(value, typeof(string));
			TitleFormatter(null, e);
			return e.Value as string;
		}

		public static string Format(DateTime? value)
		{
			ConvertEventArgs e = new ConvertEventArgs(value, typeof(string));
			DateTimeOptionalFormatter(null, e);
			return e.Value as string;
		}

		#endregion

		#region Binding formatters

		public static void TitleFormatter(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType != typeof(string))
				return;

			string value = e.Value as string;

			e.Value = string.IsNullOrEmpty(value)
				? "(UNNAMED)"
				: e.Value = value.ToUpper();
		}

		public static void DateTimeOptionalFormatter(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType != typeof(string))
				return;

			DateTime? dt = e.Value as DateTime?;

			e.Value = dt.HasValue
				? string.Format("{0:yyyy-MM-dd HH:mm:ss zzzz}", dt.Value.ToLocalTime())
				: "Never";
		}

		#endregion
	}
}
