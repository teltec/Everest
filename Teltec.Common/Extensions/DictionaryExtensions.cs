using System;
using System.Collections.Generic;
using System.IO;

namespace Teltec.Common.Extensions
{
	public static class DictionaryExtensions
	{
		#region Dictionary<string, string>

		public static bool ReadFromFile(this Dictionary<string, string> obj, string filepath)
		{
			string[] lines = new string[0];
			try
			{
				lines = File.ReadAllLines(filepath);
			}
			catch (Exception ex)
			{
				return false;
			}
			
			foreach (string line in lines)
			{
				string str = line;

				if (!str.Contains("="))
					continue;

				str = str.Replace("\r", "");

				str = str.Trim();
				if (string.IsNullOrEmpty(str) || str.StartsWith("#") || str.StartsWith(";") || str.StartsWith("'")) // Is comment line?
					continue;

				string[] kv = str.Split('=');
				string key = kv[0].Trim();
				string value = kv[1].Trim();

				obj.Add(key, value);
			}

			return true;
		}

		public static void WriteToFile(this Dictionary<string, string> obj, string filepath)
		{
			using (StreamWriter sw = new StreamWriter(filepath))
			{
				foreach (var kv in obj)
				{
					sw.Write("{0} = {1}\n", kv.Key, kv.Value);
				}
				sw.Flush();
			}
		}

		#endregion
	}
}
