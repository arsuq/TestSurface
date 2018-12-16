using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TestRunner
{
	public static class FormatText
	{
		public static string JoinLines(int leftSpaces, params string[] lines)
		{
			var sb = new StringBuilder();
			var pad = string.Empty;

			if (leftSpaces > 0)
			{
				var lp = new char[leftSpaces];
				for (int k = 0; k < lp.Length; k++) lp[k] = ' ';
				pad = new string(lp);
			}

			foreach (var l in lines)
				sb.AppendLine(pad + l);

			return sb.ToString();
		}

		public static string JoinLines(params string[] lines)
		{
			var sb = new StringBuilder();

			foreach (var l in lines)
				sb.AppendLine(l);

			return sb.ToString();
		}
	}
}
