using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace Teltec.Common.Extensions
{
	public static class StringExtensions
	{
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
