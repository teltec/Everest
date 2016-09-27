/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace Teltec.Everest.Ipc.Protocol
{
	public class Message
	{
		public string RawMessage { get; internal set; }
		public int CurrentIndex = 0;

		public Message()
		{
			RawMessage = "";
		}

		public Message(string message)
		{
			string trimmed = message.Trim();
			if (string.IsNullOrEmpty(trimmed))
				throw new ArgumentException("trimmed message cannot be null or empty", "message");
			RawMessage = trimmed;
		}

		public void Append(string token)
		{
			RawMessage += ' ' + token;
		}

		public string NextToken()
		{
			if (CurrentIndex > RawMessage.Length)
				return null;

			int newIndex = RawMessage.IndexOf(' ', CurrentIndex);
			if (newIndex < 0)
				return RemainingTokens();

			string result = Sub(RawMessage, CurrentIndex, newIndex);
			CurrentIndex = newIndex + 1; // +1 to skip the ' '
			return result;
		}

		public string RemainingTokens()
		{
			string result = Sub(RawMessage, CurrentIndex, RawMessage.Length);
			CurrentIndex = RawMessage.Length;
			return result;
		}

		private static string Sub(string raw, int start, int end)
		{
			if (raw == null)
				throw new ArgumentException("raw cannot be null", "raw");
			if (start < 0)
				throw new ArgumentException("start cannot be negative", "start");
			if (end < 0)
				throw new ArgumentException("end cannot be negative", "end");
			if (end < start)
				throw new ArgumentException("end cannot be lower than start", "end");
			if (start > raw.Length)
				throw new ArgumentException("start is past the end", "start");
			if (end > raw.Length)
				throw new ArgumentException("end is past the end", "end");
			if (start == end)
				return "";

			string result = raw.Substring(start, end - start);
			return result;
		}
	}
}
