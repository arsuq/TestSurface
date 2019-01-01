using System.Text;

namespace TestSurface
{
	public static class FormatText
	{
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
