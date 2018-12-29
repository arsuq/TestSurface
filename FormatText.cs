using System;
using System.Text;

namespace TestRunner
{
	public static class FormatText
	{
		public static string JoinLines(int leftSpaces, params string[] lines)
		{
			if (leftSpaces < 1) throw new ArgumentOutOfRangeException("leftSpaces");
			if (lines == null || lines.Length < 1) return string.Empty;

			var sb = new StringBuilder();
			var pad = string.Empty;
			var lp = new char[leftSpaces];
			for (int k = 0; k < lp.Length; k++) lp[k] = ' ';
			pad = new string(lp);

			foreach (var l in lines)
				sb.AppendLine(pad + l);

			return sb.ToString();
		}

		public static string JoinLines(params string[] lines)
		{
			if (lines == null || lines.Length < 1) return string.Empty;

			var sb = new StringBuilder();

			foreach (var l in lines)
				sb.AppendLine(l);

			return sb.ToString();
		}
	}
}
