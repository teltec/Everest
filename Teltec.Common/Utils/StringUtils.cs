using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
