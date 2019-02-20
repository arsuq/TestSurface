using System;
using System.Threading;

namespace TestSurface
{
	public static class Print
	{
		static Print()
		{
			Foreground = Console.ForegroundColor;
			Background = Console.BackgroundColor;
		}

		public static void AsHelp(this string text, params object[] formatArgs) =>
			trace(text, 0, false, ForegroundError, Background, formatArgs);

		public static void AsSystemTrace(this string text, params object[] formatArgs) =>
			trace(text, 0, false, ForegroundSystemTrace, BackgroundSystemTrace, formatArgs);

		public static void AsTestSuccess(this string text, params object[] formatArgs) =>
			trace(text, 0, false, ForegroundTestSuccess, BackgroundTestSuccess, formatArgs);

		public static void AsTestInfo(this string text, params object[] formatArgs) =>
			Trace(text, 0, false, ForegroundTestInfo, BackgroundTestInfo, formatArgs);

		public static void AsTestFailure(this string text, params object[] formatArgs) =>
			trace(text, 0, false, ForegroundTestFailure, BackgroundTestFailure, formatArgs);

		public static void AsTestUnknown(this string text, params object[] formatArgs) =>
			trace(text, 0, false, ForegroundTestUnknown, BackgroundTestUnknown, formatArgs);

		public static void AsTestHeader(this string text, params object[] formatArgs) =>
			trace(text, 0, false, ForegroundTestHeader, BackgroundTestHeader, formatArgs);

		public static void AsInfo(this string text, ConsoleColor c, params object[] formatArgs) =>
			Trace(text, 2, false, c, Background, formatArgs);

		public static void AsInfo(this string text, int leftMargin, bool split, ConsoleColor c, params object[] formatArgs) =>
			Trace(text, leftMargin, split, c, Background, formatArgs);

		public static void AsInfo(this string text, params object[] formatArgs) =>
			Trace(text, 2, false, Foreground, Background, formatArgs);

		public static void AsSuccess(this string text, params object[] formatArgs) =>
			Trace(text, 2, false, ForegroundSuccess, Background, formatArgs);

		public static void AsInnerInfo(this string text, params object[] formatArgs) =>
			Trace(text, 4, false, Foreground, Background, formatArgs);

		public static void AsError(this string text, params object[] formatArgs) =>
			Trace(text, 2, false, ForegroundError, Background, formatArgs);

		public static void AsWarn(this string text, params object[] formatArgs) =>
			Trace(text, 2, false, ForegroundWarn, Background, formatArgs);

		public static void Trace(this string text, ConsoleColor c, params object[] formatArgs) =>
			Trace(text, 2, false, c, Background, formatArgs);

		/// <summary>
		/// Traces the text if both IgnoreInfo and IgnoreAll are false.
		/// </summary>
		/// <param name="text">The text string.</param>
		/// <param name="leftMargin">Number of chars to pad each line.</param>
		/// <param name="split">Splits the text into multiple lines and adds the margin.</param>
		/// <param name="c">The trace foreground color.</param>
		/// <param name="bg">The trace background color. </param>
		/// <param name="formatArgs">The standard string format arguments.</param>
		public static void Trace(this string text, int leftMargin, bool split, ConsoleColor c, ConsoleColor bg, params object[] formatArgs)
		{
			if (IgnoreInfo) return;

			trace(text, leftMargin, split, c, bg, formatArgs);
		}

		internal static void trace(this string text, int leftMargin, bool split, ConsoleColor c, ConsoleColor bg, params object[] formatArgs)
		{
			if (IgnoreAll) return;

			var L = split ? text.Split(SPLIT_LINE, SplitOptions) : null;
			var pass = false;

			if (SerializeTraces) spinLock.TryEnter(LockAwaitMS, ref pass);
			else pass = true;

			if (pass)
			{
				var cc = Console.ForegroundColor;
				var bc = Console.BackgroundColor;
				Console.ForegroundColor = c;
				Console.BackgroundColor = bg;
				Console.SetCursorPosition(leftMargin, Console.CursorTop);
				if (L != null)
					foreach (var line in L)
					{
						Console.SetCursorPosition(leftMargin, Console.CursorTop);
						Console.WriteLine(line, formatArgs);
					}
				else Console.WriteLine(text, formatArgs);
				Console.ForegroundColor = cc;
				Console.BackgroundColor = bc;

				if (SerializeTraces) spinLock.Exit();
			}
			else if (ThrowOnLockTimeout)
				throw new TimeoutException($"Failed to acquire the trace lock in {LockAwaitMS}ms.");
		}

		/// <summary>
		/// The text splitter string.
		/// The default is Environment.NewLine.
		/// </summary>
		public static string[] SPLIT_LINE = new string[] { Environment.NewLine };

		/// <summary>
		/// By default is set to StringSplitOptions.None.
		/// </summary>
		public static StringSplitOptions SplitOptions = StringSplitOptions.None;

		/// <summary>
		/// Never traces if true.
		/// </summary>
		public static bool IgnoreAll = false;

		/// <summary>
		/// Ignores all but the SystemTrace and the test header and status
		/// </summary>
		public static bool IgnoreInfo = false;

		/// <summary>
		/// Will bail tracing after waiting the specified timeout in milliseconds.
		/// If ThrowOnLockTimeout is true will throw a TimeoutException.
		/// The default value is 100.
		/// </summary>
		public static int LockAwaitMS = 100;

		/// <summary>
		/// By default is true.
		/// </summary>
		public static bool ThrowOnLockTimeout = true;

		/// <summary>
		/// If there is no threading involved set to false.
		/// Starts as true.
		/// </summary>
		public static bool SerializeTraces = true;


		public static ConsoleColor ForegroundHelp = ConsoleColor.DarkCyan;
		public static ConsoleColor ForegroundSuccess = ConsoleColor.Green;
		public static ConsoleColor ForegroundError = ConsoleColor.Red;
		public static ConsoleColor ForegroundWarn = ConsoleColor.Yellow;

		public static ConsoleColor ForegroundSystemTrace = ConsoleColor.White;
		public static ConsoleColor BackgroundSystemTrace = ConsoleColor.DarkBlue;
		public static ConsoleColor ForegroundTestSuccess = ConsoleColor.White;
		public static ConsoleColor BackgroundTestSuccess = ConsoleColor.DarkGreen;
		public static ConsoleColor ForegroundTestInfo = ConsoleColor.Yellow;
		public static ConsoleColor BackgroundTestInfo = ConsoleColor.Black;
		public static ConsoleColor ForegroundTestFailure = ConsoleColor.White;
		public static ConsoleColor BackgroundTestFailure = ConsoleColor.DarkRed;
		public static ConsoleColor ForegroundTestUnknown = ConsoleColor.White;
		public static ConsoleColor BackgroundTestUnknown = ConsoleColor.DarkMagenta;
		public static ConsoleColor ForegroundTestHeader = ConsoleColor.Black;
		public static ConsoleColor BackgroundTestHeader = ConsoleColor.White;

		public static ConsoleColor Foreground = ConsoleColor.White;
		public static ConsoleColor Background = ConsoleColor.Black;

		static SpinLock spinLock = new SpinLock();
	}
}