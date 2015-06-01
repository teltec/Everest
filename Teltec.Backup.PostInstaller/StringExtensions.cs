using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace PostInstaller
{
	public static class StringExtensions
	{
		static readonly Regex VariableRegex = new Regex(@"\{\{([^\}]+)\}\}", RegexOptions.Compiled);

		public static string ExpandVariables(this string input, StringDictionary variables)
		{
			string output = VariableRegex.Replace(input, delegate(Match match)
			{
				return variables[match.Groups[1].Value];
			});
			return output;
		}
	}
}
