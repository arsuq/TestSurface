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
			var lp = new char[leftSpaces];

			for (int i = 0; i < lp.Length; i++)
				lp[i] = ' ';

			var pad = new string(lp);

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
