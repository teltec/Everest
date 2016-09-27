/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NUnit.Framework;
using System;
using System.Linq;
using System.Text;

namespace Teltec.Common.Extensions
{
	public static class RandomExtensions
	{
		public static string GenerateRandomHexaString(this Random r, int length)
		{
			byte[] alphabet = "0123456789ABCDEF".Select(v => (byte)v).ToArray();

			return GenerateRandomString(r, length, alphabet);
		}

		public static string GenerateRandomPrintableString(this Random r, int length)
		{
			//byte[] all_ascii = Enumerable.Range(0, 128).Select(v => (byte)v).ToArray(); // Interval [0, 127]
			byte[] alphabet = Enumerable.Range(32, 95).Select(v => (byte)v).ToArray(); // Interval [32, 126]

			return GenerateRandomString(r, length, alphabet);
		}

		public static string GenerateRandomString(this Random r, int length, byte[] alphabet)
		{
			Assert.IsTrue(length > 0);
			Assert.IsTrue(alphabet.Length > 1);

			var data = new byte[length];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)alphabet[r.Next(0, alphabet.Length)];
			}
			var encoding = new ASCIIEncoding();
			return encoding.GetString(data);
		}
	}
}
