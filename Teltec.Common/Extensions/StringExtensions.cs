/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Teltec.Common.Extensions
{
	public static class StringExtensions
	{
		static readonly Regex VariableRegex = new Regex(@"\{\{([^\}]+)\}\}", RegexOptions.Compiled);

		public static string ExpandVariables(this string input, StringDictionary variables)
		{
			string output = VariableRegex.Replace(input, delegate(Match match)
			{
				return variables[match.Groups[1].Value];
			});
			return output;
		}

		public static string ToTitleCase(this string text)
		{
			CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
			TextInfo textInfo = cultureInfo.TextInfo;
			return textInfo.ToTitleCase(text);
		}

		// ORIGINAL CODE FROM http://www.levibotelho.com/development/c-remove-diacritics-accents-from-a-string/
		// Copyright (c) 2015 Levi Botelho.
		public static string RemoveDiacritics(this string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return text;

			text = text.Normalize(NormalizationForm.FormD);
			var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
			return new string(chars).Normalize(NormalizationForm.FormC);
		}

		public static Nullable<T> ToNullableEnum<T>(this string value, Nullable<T> defaultValue) where T : struct
		{
			if (string.IsNullOrEmpty(value))
				return defaultValue;

			T result;
			return Enum.TryParse<T>(value, out result) ? result : defaultValue;
		}

		public static T ToEnum<T>(this string value, T defaultValue) where T : struct
		{
			if (string.IsNullOrEmpty(value))
				return defaultValue;

			T result;
			return Enum.TryParse<T>(value, out result) ? result : defaultValue;
		}
	}
}
