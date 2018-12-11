﻿using System;
using System.Collections.Generic;

namespace TestRunner
{
	public class ArgsParser
	{
		public const char SWITCH_SYMBOL = '-';

		public ArgsParser(string[] args)
		{
			if (args == null) throw new ArgumentNullException("args");

			if (args.Length > 0)
			{
				var sw = string.Empty;

				foreach (var arg in args)
				{
					if (arg[0] == SWITCH_SYMBOL)
					{
						sw = arg;
						argsMap.Add(arg, new List<string>());
					}
					else if (!string.IsNullOrEmpty(sw))
						argsMap[sw].Add(arg);
				}
			}
		}


		public IDictionary<string, List<string>> Map => argsMap;

		Dictionary<string, List<string>> argsMap = new Dictionary<string, List<string>>();
	}

	public static class ArgsParserExt
	{
		public static void AssertAll(this IDictionary<string, List<string>> map, params string[] mandatorySwitches)
		{
			if (map == null) throw new ArgumentNullException("map");
			if (mandatorySwitches == null) throw new ArgumentNullException("mandatorySwitches");

			foreach (var sw in mandatorySwitches)
				if (!map.ContainsKey(sw))
					throw new ArgumentException(sw);
		}

		public static void AssertAtLeastOneArg(this List<string> args, params string[] possibleValues)
		{
			if (args == null) throw new ArgumentNullException("args");
			if (possibleValues == null) throw new ArgumentNullException("possibleValues");

			foreach (var v in possibleValues)
				if (!args.Exists((e) => e == v))
					throw new ArgumentException(v);
		}

		public static void AssertNothingButTheseArgs(this List<string> args, params string[] possibleValues)
		{
			if (args == null) throw new ArgumentNullException("args");
			if (possibleValues == null) throw new ArgumentNullException("possibleValues");

			foreach (var v in possibleValues)
				if (!Array.Exists(possibleValues, (e) => e == v))
					throw new ArgumentException(v);
		}
	}
}