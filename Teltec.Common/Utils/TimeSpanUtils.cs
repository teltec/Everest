using System;
using System.Collections.Generic;
using System.Linq;

namespace Teltec.Common.Utils
{
	public class TimeSpanUtils
	{
		//
		// "How to produce “human readable” strings to represent a TimeSpan" by "rene" is licensed under CC BY-SA 3.0
		//
		// Title?   How to produce “human readable” strings to represent a TimeSpan
		// Author?  rene - http://stackoverflow.com/users/578411/rene
		// Source?  http://stackoverflow.com/a/21649465/298054
		// License? CC BY-SA 3.0 - https://creativecommons.org/licenses/by-sa/3.0/legalcode
		//
		public static string GetReadableTimespan(TimeSpan ts)
		{
			// Formats and its cutoffs based on totalseconds
			var cutoff = new SortedList<long, string> {
				{ 60,				"{3:S}"			},
				{ 60 * 60,			"{2:M}, {3:S}"	},
				{ 24 * 60 * 60,		"{1:H}, {2:M}"	},
				{ Int64.MaxValue,	"{0:D}, {1:H}"	},
			};

			// Find nearest best match
			var find = cutoff.Keys.ToList().BinarySearch((long)ts.TotalSeconds);

			// Negative values indicate a nearest match
			var near = find < 0 ? Math.Abs(find) - 1 : find;

			// Use custom formatter to get the string
			return String.Format(
				new HMSFormatter(),
				cutoff[cutoff.Keys[near]],
				ts.Days,
				ts.Hours,
				ts.Minutes,
				ts.Seconds);
		}
	}

	//
	// "How to produce “human readable” strings to represent a TimeSpan" by "rene" is licensed under CC BY-SA 3.0
	//
	// Title?   How to produce “human readable” strings to represent a TimeSpan
	// Author?  rene - http://stackoverflow.com/users/578411/rene
	// Source?  http://stackoverflow.com/a/21649465/298054
	// License? CC BY-SA 3.0 - https://creativecommons.org/licenses/by-sa/3.0/legalcode
	//
	// Formatter for plural/singular forms of seconds/hours/days.
	public class HMSFormatter : ICustomFormatter, IFormatProvider
	{
		string _plural, _singular;

		public HMSFormatter() { }

		private HMSFormatter(string plural, string singular)
		{
			_plural = plural;
			_singular = singular;
		}

		public object GetFormat(Type formatType)
		{
			return formatType == typeof(ICustomFormatter) ? this : null;
		}

		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			if (arg != null)
			{
				string fmt;
				switch (format)
				{
					case "S": // second
						fmt = String.Format(new HMSFormatter("{0} seconds", "{0} second"), "{0}", arg);
						break;
					case "M": // minute
						fmt = String.Format(new HMSFormatter("{0} minutes", "{0} minute"), "{0}", arg);
						break;
					case "H": // hour
						fmt = String.Format(new HMSFormatter("{0} hours", "{0} hour"), "{0}", arg);
						break;
					case "D": // day
						fmt = String.Format(new HMSFormatter("{0} days", "{0} day"), "{0}", arg);
						break;
					default:
						// plural/singular
						fmt = String.Format((int)arg > 1 ? _plural : _singular, arg);  // watch the cast to int here...
						break;
				}
				return fmt;
			}
			return String.Format(format, arg);
		}
	}
}
