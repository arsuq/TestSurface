using System;
using System.Collections.Generic;

namespace TestSurface
{
	public static class ArgsParser
	{
		/// <summary>
		/// Will return a map with keys the args elements prefixed with argPrf 
		/// and values all strings until the next option or the end of the array.
		/// Values without an option are placed in the defaultKey list.
		/// </summary>
		/// <param name="defaultKey">For values without a leading switch.</param>
		/// <param name="argPrf">The switches prefix.</param>
		/// <param name="args">The values to be arranged.</param>
		/// <returns></returns>
		public static Dictionary<string, List<string>> Parse(string defaultKey, char argPrf, params string[] args)
		{
			var argsMap = new Dictionary<string, List<string>>();

			if (args != null && args.Length > 0)
			{
				argsMap.Add(defaultKey, new List<string>());
				var sw = string.Empty;

				foreach (var arg in args)
				{
					if (arg[0] == argPrf)
					{
						sw = arg;
						argsMap.Add(arg, new List<string>());
					}
					else if (!string.IsNullOrEmpty(sw)) argsMap[sw].Add(arg);
					else argsMap[defaultKey].Add(arg);
				}
			}

			return argsMap;
		}

		public static void AssertAll(this IDictionary<string, List<string>> map, params string[] mandatorySwitches)
		{
			if (map == null) throw new ArgumentNullException("map");
			if (mandatorySwitches == null) throw new ArgumentNullException("mandatorySwitches");

			foreach (var sw in mandatorySwitches)
				if (!map.ContainsKey(sw))
					throw new ArgumentException(sw);
		}

		public static void AssertAtLeastOne(this List<string> args, params string[] possibleValues)
		{
			if (args == null) throw new ArgumentNullException("args");
			if (possibleValues == null) throw new ArgumentNullException("possibleValues");

			foreach (var v in possibleValues)
				foreach (var a in args)
					if (a == v) return;

			throw new ArgumentException();
		}

		public static void AssertNothingOutsideThese(this List<string> args, params string[] possibleValues)
		{
			if (args == null) throw new ArgumentNullException("args");
			if (possibleValues == null) throw new ArgumentNullException("possibleValues");

			foreach (var a in args)
			{
				var match = false;

				foreach (var v in possibleValues)
					if (a == v)
					{
						match = true;
						break;
					};

				if (!match) throw new ArgumentException();
			}
		}
	}
}
