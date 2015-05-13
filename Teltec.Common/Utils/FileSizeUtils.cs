using System.Windows.Forms;

namespace Teltec.Common.Utils
{
	public static class FileSizeUtils
	{
		private static readonly long Byte = 1;
		private static readonly long Kilo = 1024 * Byte;
		private static readonly long Mega = 1024 * Kilo;
		private static readonly long Giga = 1024 * Mega;
		private static readonly long Peta = 1024 * Giga;

		private class UnitDescriptor
		{
			public long Value;
			public string Format;
			public string Unit;
		}

		private static readonly UnitDescriptor[] Units = new UnitDescriptor[] {
			new UnitDescriptor { Value=Peta, Format="{0:F2} {1}", Unit="PB" },
			new UnitDescriptor { Value=Giga, Format="{0:F2} {1}", Unit="GB" },
			new UnitDescriptor { Value=Mega, Format="{0:F2} {1}", Unit="MB" },
			new UnitDescriptor { Value=Kilo, Format="{0:F2} {1}", Unit="KB" },
			new UnitDescriptor { Value=Byte, Format="{0} {1}"   , Unit="bytes" },
		};

		static public string FileSizeToString(long size)
		{
			if (size == 0)
				return "Completed";
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
