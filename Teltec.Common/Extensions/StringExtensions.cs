using System.Globalization;
using System.Linq;
using System.Text;

namespace Teltec.Common.Extensions
{
	public static class StringExtensions
	{
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
	}
}
