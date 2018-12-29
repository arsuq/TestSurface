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
			trace(text, ConsoleColor.DarkCyan, BC, formatArgs);

		public static void AsSystemTrace(this string text, params object[] formatArgs) =>
			trace(text, ConsoleColor.White, ConsoleColor.DarkBlue, formatArgs);

		public static void AsTestSuccess(this string text, params object[] formatArgs) =>
			trace(text, ConsoleColor.White, ConsoleColor.DarkGreen, formatArgs);

		public static void AsTestInfo(this string text, params object[] formatArgs) =>
			Trace(text, ConsoleColor.Yellow, null, formatArgs);

		public static void AsTestFailure(this string text, params object[] formatArgs) =>
			trace(text, ConsoleColor.White, ConsoleColor.DarkRed, formatArgs);

		public static void AsTestUnknown(this string text, params object[] formatArgs) =>
			trace(text, ConsoleColor.White, ConsoleColor.DarkMagenta, formatArgs);

		public static void AsTestHeader(this string text, params object[] formatArgs) =>
			trace(text, ConsoleColor.Black, ConsoleColor.White, formatArgs);

		public static void AsInfo(this string text, ConsoleColor c, params object[] formatArgs) =>
			Trace(text, c, BC, formatArgs);

		public static void AsInfo(this string text, params object[] formatArgs) =>
			Trace(text, FC, BC, formatArgs);

		public static void AsSuccess(this string text, params object[] formatArgs) =>
			Trace(text, ConsoleColor.Green, BC, formatArgs);

		public static void AsInnerInfo(this string text, params object[] formatArgs) =>
			Trace("    " + text, FC, BC, formatArgs);

		public static void AsError(this string text, params object[] formatArgs) =>
			Trace(text, ConsoleColor.Red, BC, formatArgs);

		public static void AsWarn(this string text, params object[] formatArgs) =>
			Trace(text, ConsoleColor.Yellow, BC, formatArgs);

		public static void Trace(this string text, ConsoleColor c, params object[] formatArgs) =>
			Trace(text, c, BC, formatArgs);

		public static void Trace(this string text, ConsoleColor c, ConsoleColor bg, params object[] formatArgs)
		{
			if (IgnoreInfo) return;

			trace(text, c, bg, formatArgs);
		}

		internal static void trace(this string text, ConsoleColor c, ConsoleColor bg, params object[] formatArgs)
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
				Console.WriteLine(text, formatArgs);
				Console.ForegroundColor = cc;
				Console.BackgroundColor = bc;

				spinLock.Exit();
			}
		}

		public static bool IgnoreAll = false;
		public static bool IgnoreInfo = false;
		static SpinLock spinLock = new SpinLock();
		static ConsoleColor FC = ConsoleColor.Black;
		static ConsoleColor BC = ConsoleColor.Black;
	}
}
