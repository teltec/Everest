using System.Runtime.CompilerServices;

namespace Teltec.Common.Utils
{
	public static class RuntimeUtils
	{
		// Summary:
		//   Return the name of the method that is calling it.
		//
		//   The caller me be annotated with `[MethodImpl(MethodImplOptions.NoInlining)]`,
		//	 otherwise it might be inlined and the returned value might not be what you're
		//   expecting.
		//[MethodImpl(MethodImplOptions.NoInlining)]
		//public static string GetCurrentMethodNameViaFrame()
		//{
		//	StackTrace st = new StackTrace();
		//	StackFrame sf = st.GetFrame(1);
		//
		//	return sf.GetMethod().Name;
		//}

		public static string GetCurrentMethodName([CallerMemberName] string memberName = "")
		{
			return memberName;
		}

		public static string GetCurrentSourceFilePath([CallerFilePath] string sourceFilePath = "")
		{
			return sourceFilePath;
		}

		public static int GetCurrentSourceLineNumber([CallerLineNumber] int sourceLineNumber = 0)
		{
			return sourceLineNumber;
		}

		public static string GetSourceIdentifier([CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return sourceFilePath + ":" + sourceLineNumber + "/" + memberName;
		}
	}
}
