using System;
using System.Threading;

namespace TestRunner
{
	public static class Print
	{
		static Print()
		{
			FC = Console.ForegroundColor;
			BC = Console.BackgroundColor;
		}

		public static void AsHelp(this string text, params object[] formatArgs) =>
			trace(text, 0, false, ConsoleColor.DarkCyan, BC, formatArgs);

		public static void AsSystemTrace(this string text, params object[] formatArgs) =>
			trace(text, 0, false, ConsoleColor.White, ConsoleColor.DarkBlue, formatArgs);

		public static void AsTestSuccess(this string text, params object[] formatArgs) =>
			trace(text, 0, false, ConsoleColor.White, ConsoleColor.DarkGreen, formatArgs);

		public static void AsTestInfo(this string text, params object[] formatArgs) =>
			Trace(text, 0, false, ConsoleColor.Yellow, null, formatArgs);

		public static void AsTestFailure(this string text, params object[] formatArgs) =>
			trace(text, 0, false, ConsoleColor.White, ConsoleColor.DarkRed, formatArgs);

		public static void AsTestUnknown(this string text, params object[] formatArgs) =>
			trace(text, 0, false, ConsoleColor.White, ConsoleColor.DarkMagenta, formatArgs);

		public static void AsTestHeader(this string text, params object[] formatArgs) =>
			trace(text, 0, false, ConsoleColor.Black, ConsoleColor.White, formatArgs);

		public static void AsInfo(this string text, ConsoleColor c, params object[] formatArgs) =>
			Trace(text, 2, false, c, BC, formatArgs);

		public static void AsInfo(this string text, int leftMargin, bool split, ConsoleColor c, params object[] formatArgs) =>
			Trace(text, leftMargin, split, c, BC, formatArgs);

		public static void AsInfo(this string text, params object[] formatArgs) =>
			Trace(text, 2, false, FC, BC, formatArgs);

		public static void AsSuccess(this string text, params object[] formatArgs) =>
			Trace(text, 2, false, ConsoleColor.Green, BC, formatArgs);

		public static void AsInnerInfo(this string text, params object[] formatArgs) =>
			Trace(text, 4, false, FC, BC, formatArgs);

		public static void AsError(this string text, params object[] formatArgs) =>
			Trace(text, 2, false, ConsoleColor.Red, BC, formatArgs);

		public static void AsWarn(this string text, params object[] formatArgs) =>
			Trace(text, 2, false, ConsoleColor.Yellow, BC, formatArgs);

		public static void Trace(this string text, ConsoleColor c, params object[] formatArgs) =>
			Trace(text, 2, false, c, BC, formatArgs);

		public static void Trace(this string text, int leftMargin, bool split, ConsoleColor c, ConsoleColor bg, params object[] formatArgs)
		{
			if (IgnoreInfo) return;

			trace(text, leftMargin, split, c, bg, formatArgs);
		}

		internal static void trace(this string text, int leftMargin, bool split, ConsoleColor c, ConsoleColor bg, params object[] formatArgs)
		{
			if (IgnoreAll) return;

			bool locked = false;
			spinLock.Enter(ref locked);

			if (locked)
			{
				var cc = Console.ForegroundColor;
				var bc = Console.BackgroundColor;
				Console.ForegroundColor = c;
				Console.BackgroundColor = bg;
				Console.SetCursorPosition(leftMargin, Console.CursorTop);
				if (split)
					foreach (var line in text.Split(SPLIT_LINE, StringSplitOptions.None))
					{
						Console.SetCursorPosition(leftMargin, Console.CursorTop);
						Console.WriteLine(line, formatArgs);
					}
				else Console.WriteLine(text, formatArgs);
				Console.ForegroundColor = cc;
				Console.BackgroundColor = bc;

				spinLock.Exit();
			}
		}

		public static readonly string[] SPLIT_LINE = new string[] { Environment.NewLine };
		public static bool IgnoreAll = false;
		public static bool IgnoreInfo = false;
		static SpinLock spinLock = new SpinLock();
		static ConsoleColor FC = ConsoleColor.Black;
		static ConsoleColor BC = ConsoleColor.Black;
	}
}
