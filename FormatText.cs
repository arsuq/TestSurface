using System;
using System.Collections.Generic;
using System.Text;

namespace TestRunner
{
	public static class FormatText
	{
		public static string JoinLines(params string[] lines)
		{
			var sb = new StringBuilder();

			foreach (var l in lines)
				sb.AppendLine(l);

			return sb.ToString();
		}
	}
}
