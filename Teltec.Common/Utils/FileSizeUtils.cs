using System;
using System.Windows.Forms;

namespace Teltec.Common.Utils
{
	public static class FileSizeUtils
	{
		//
		// "How do you do *integer* exponentiation in C#?" by "Vilx-" is licensed under CC BY-SA 3.0
		//
		// Title?   How do you do *integer* exponentiation in C#?
		// Author?  Vilx- - http://stackoverflow.com/users/41360/vilx
		// Source?  http://stackoverflow.com/a/383596/298054
		// License? CC BY-SA 3.0 - https://creativecommons.org/licenses/by-sa/3.0/legalcode
		//
		public static long LongPow(long x, ulong pow)
		{
			try
			{
				checked
				{
					long ret = 1;
					while (pow != 0)
					{
						if ((pow & 1) == 1)
							ret *= x;
						x *= x;
						pow >>= 1;
					}
					return ret;
				}
			}
			catch (OverflowException)
			{
				throw;
			}
		}

		private static readonly long Byte = 1;
		private static readonly long Kilo = LongPow(10, 3);
		private static readonly long Mega = LongPow(10, 6);
		private static readonly long Giga = LongPow(10, 9);
		private static readonly long Peta = LongPow(10, 12);
		private static readonly long Exa = LongPow(10, 15);

		private class UnitDescriptor
		{
			public long Value;
			public string Format;
			public string Unit;
		}

		private static readonly UnitDescriptor[] Units = new UnitDescriptor[] {
			new UnitDescriptor { Value=Exa, Format="{0:F2} {1}" , Unit="EB" },
			new UnitDescriptor { Value=Peta, Format="{0:F2} {1}", Unit="PB" },
			new UnitDescriptor { Value=Giga, Format="{0:F2} {1}", Unit="GB" },
			new UnitDescriptor { Value=Mega, Format="{0:F2} {1}", Unit="MB" },
			new UnitDescriptor { Value=Kilo, Format="{0:F2} {1}", Unit="kB" },
			new UnitDescriptor { Value=Byte, Format="{0} {1}"   , Unit="bytes" },
		};

		static public string FileSizeToString(long size)
		{
			if (size == 0)
				return "0 bytes";

			foreach (UnitDescriptor unit in Units)
			{
				if (size >= unit.Value)
				{
					double remaining = (double)size / unit.Value;
					return string.Format(unit.Format, remaining, unit.Unit);
				}
			}

			return "Unknown";
		}

		static public void FileSizeToString(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(string))
			{
				e.Value = FileSizeToString((long)e.Value);
			}
		}
	}
}
