using System;

namespace Teltec.Common.Utils
{
	public static class DateTimeUtils
	{
		//
		// "How to round-off hours based on Minutes(hours+0 if min<30, hours+1 otherwise)?" by "CrimsonX" is licensed under CC BY-SA 3.0
		//
		// Title?   How to round-off hours based on Minutes(hours+0 if min<30, hours+1 otherwise)?
		// Author?  CrimsonX - http://stackoverflow.com/users/181211/crimsonx
		// Source?  http://stackoverflow.com/a/2906684/298054
		// License? CC BY-SA 3.0 - https://creativecommons.org/licenses/by-sa/3.0/legalcode
		//
		/// <summary>
		/// Rounds (up or down) a DateTime to the nearest hour.
		/// </summary>
		/// <param name="dateTime">DateTime to Round</param>
		/// <returns>DateTime rounded to nearest hour</returns>
		public static DateTime RoundToNearestHour(this DateTime dateTime)
		{
		  dateTime += TimeSpan.FromMinutes(30);

		  return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, dateTime.Kind);
		}

		public static DateTime RoundUpToQuarterHours(this DateTime dateTime)
		{
			return RoundUp(dateTime, TimeSpan.FromMinutes(15));
		}

		public static DateTime RoundUpToHalfHours(this DateTime dateTime)
		{
			return RoundUp(dateTime, TimeSpan.FromMinutes(30));
		}

		public static DateTime RoundUp(DateTime dt, TimeSpan d)
		{
			return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
		}
	}
}
