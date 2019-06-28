using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestSurface
{
	/// <summary>
	/// The ITestSurface launcher.
	/// </summary>
	public class SurfaceLauncher
	{
		public SurfaceLauncher()
		{
			RunHistory = new List<RunRecord>();
			SurfaceTypes = AppDomain.CurrentDomain
				.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => typeof(ITestSurface).IsAssignableFrom(p) && p.IsClass)
				.ToDictionary(x => ARG_SWITCH + x.Name, y => y);

			if (SurfaceTypes.Count < 1) "No ITestSurface implementations were found;".AsWarn();
		}

		/// <summary>
		/// Starts all discoverable tests. 
		/// </summary>
		/// <remarks>
		/// Only one Start execution at a time is allowed.
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">If another Start is still executing.</exception>
		/// <param name="args">The arguments list</param>
		public void Start(params string[] args)
		{
			if (Interlocked.CompareExchange(ref isRuning, 1, 0) > 0) throw new InvalidOperationException("Tests are running");

			try
			{
				if (args == null || args.Length < 1) { "Nothing to launch.".AsWarn(); return; }

				var runIdx = RunHistory.Count + 1;
				var runRec = new RunRecord(runIdx, args);
				var argsMap = ArgsParser.Parse(DEF_OPTION_KEY, ARG_SWITCH, args);
				var cmdMode = argsMap.ContainsKey(CMD);

				if (cmdMode)
				{
					cmd(argsMap);
					return;
				}

				var runAll = argsMap.ContainsKey(ALL);
				var breakOnFirstFailure = argsMap.ContainsKey(BREAK);
				var globalNoTrace = argsMap.ContainsKey(GLOBAL_NOTRACE);
				var runStart = DateTime.Now;
				var infoTraces = 0;
				Print.IgnoreInfo = globalNoTrace;
				Print.IgnoreAll = argsMap.ContainsKey(NO_PRINT);

				if (runIdx < 2) printRunHeader();
				RunHistory.Add(runRec);

				$"Starting test run #{runIdx} [{DateTime.Now.ToLongTimeString()}]".AsSystemTrace();
				$"Args: {String.Join(" ", args)}".AsSystemTrace();
				Print.Line();

				if (runAll)
				{
					var allArgs = ArgsParser.Parse(DEF_OPTION_KEY, SUB_ARG_SWITCH, argsMap[ALL].ToArray());

					HashSet<string> wtags = null;
					HashSet<string> wotags = null;
					HashSet<string> wxtags = null;

					if (allArgs.ContainsKey(WITH_TAGS))
						wtags = new HashSet<string>(allArgs[WITH_TAGS]);
					else if (allArgs.ContainsKey(WITHOUT_TAGS))
						wotags = new HashSet<string>(allArgs[WITHOUT_TAGS]);
					else if (allArgs.ContainsKey(WITH_EXACT_TAGS))
						wxtags = new HashSet<string>(allArgs[WITH_EXACT_TAGS]);

					var tagQry = (wtags != null || wotags != null || wxtags != null);

					foreach (var t in SurfaceTypes)
						try
						{
							var test = Activator.CreateInstance(t.Value) as ITestSurface;

							if (tagQry)
							{
								var tags = getTags(test);

								if (tags == null || wtags != null && wtags.Intersect(tags).Count() < 1) continue;
								if (wotags != null && tags != null && wotags.Intersect(tags).Count() > 0) continue;
								if (tags == null || wxtags != null && wxtags.Intersect(tags).Count() != wxtags.Count) continue;
							}

							if (test.IndependentLaunchOnly) { runRec.Skipped++; continue; }

							run(test, t.Value);
							if (breakOnFirstFailure && ((!test.Passed.HasValue) ||
								(test.Passed.HasValue && !test.Passed.Value))) break;
						}
						catch (Exception ex)
						{
							runRec.Exceptions.Add(ex);
							Print.AsError(ex.ToString());
							if (breakOnFirstFailure) break;
						}
				}
				else
					foreach (var k in argsMap)
						if ((k.Key[0] == ARG_SWITCH) && SurfaceTypes.ContainsKey(k.Key))
							try
							{
								var t = SurfaceTypes[k.Key];
								var test = Activator.CreateInstance(t) as ITestSurface;
								run(test, t);
								if (breakOnFirstFailure && ((!test.Passed.HasValue) ||
									(test.Passed.HasValue && !test.Passed.Value))) break;
							}
							catch (Exception ex)
							{
								runRec.Exceptions.Add(ex);
								Print.AsError(ex.ToString());
								if (breakOnFirstFailure) break;
							}

				void run(ITestSurface test, Type st)
				{
					if (test == null) throw new ArgumentNullException("test", st.Name);

					//Reset for every test if not set globally
					if (!globalNoTrace) Print.IgnoreInfo = false;

					var testRec = new SurfaceRunRecord(test, runIdx - 1);

					string[] ownArgs = runAll ? argsMap[ALL].ToArray() :
						argsMap.ContainsKey(ARG_SWITCH + st.Name) ?
						argsMap[ARG_SWITCH + st.Name].ToArray() : null;

					if (ownArgs != null && ownArgs.Length > 0)
					{
						// Make the test input if there are sub-switches, i.e. -sw x y
						testRec.ArgsMap = ArgsParser.Parse(DEF_OPTION_KEY, SUB_ARG_SWITCH, ownArgs);
						// If there are no sub-switches all args go with a * key 
						if (testRec.ArgsMap.Count < 1 && ownArgs.Length > 0)
							testRec.ArgsMap.Add(DEF_OPTION_KEY, new List<string>(ownArgs));
					}

					if (runAll) testRec.ArgsMap.Add(ALL, null);
					if (!globalNoTrace && testRec.ArgsMap.ContainsKey(NO_TRACE))
						Print.IgnoreInfo = true;

					var started = DateTime.Now;

					try
					{
						if (testRec.ArgsMap.ContainsKey(SKIP) &&
							(!runAll || testRec.ArgsMap[SKIP].IndexOf(st.Name) >= 0)) return;

						Print.Line();
						Print.Line();
						var input = ownArgs != null && !runAll ? String.Join(" ", ownArgs) : "";
						var header = $"+{st.Name} [{st.Assembly.GetName().Name}] {input}";

						Print.AsTestHeader(header);
						Print.AsTestHeader("".PadRight(header.Length, '-'));

						var info = testRec.ArgsMap.ContainsKey(INFO);

						if (PrintTestInfo || info)
						{
							if (!string.IsNullOrEmpty(test.Tags)) $" # {test.Tags}".Trace(0, false, Print.Tag);
							$" {test.Info}".AsTestInfo();

							if (info)
							{
								infoTraces++;
								return;
							}
						}

						Print.Line();
						test.Start(testRec.ArgsMap).Wait();
					}
					catch (Exception ex) { testRec.Exception = ex; }
					finally { testRec.Duration = DateTime.Now.Subtract(started); }

					// After the possible -info traces and skips
					runRec.Launched++;
					runRec.Tests.Add(st, testRec);
					var dur = testRec.Duration;
					var durstr = string.Format("[{0}m {1}s {2}ms]", dur.Minutes, dur.Seconds, dur.Milliseconds);

					Print.Line();

					if (test.Passed.HasValue || testRec.Exception != null)
					{
						if (test.Passed.HasValue && test.Passed.Value && testRec.Exception == null)
						{
							runRec.Passed++;
							Print.AsTestSuccess(string.Format("OK: {0} {1}", st.Name, durstr));
						}
						else
						{
							runRec.Failed++;
							Print.AsTestFailure(string.Format("FAIL: {0} {1} Ex: {2}", st.Name, durstr, test.FailureMessage));
							if (breakOnFirstFailure) return;
						}
					}
					else
					{
						runRec.Unknown++;
						Print.AsTestUnknown(string.Format("UNKNOWN: {0} {1}", st.Name, durstr));
					}
				}

				runRec.Duration = DateTime.Now.Subtract(runStart);
				Print.Line();
				Print.trace("{0,-33}", 0, false,
					Print.SystemTrace,
					$"RUN #{runIdx} RESULTS");
				runRec.PrintStats();
				Print.Line();

				if (runRec.Launched < 1 && infoTraces < 1 && args != null && args.Length > 0)
					"Nothing to launch.".AsWarn();
			}
			finally { Interlocked.Exchange(ref isRuning, 0); }
		}

		/// <summary>
		/// Aggregates all counters.
		/// </summary>
		public RunRecordStats GetTotalStats()
		{
			var rs = new RunRecordStats();

			foreach (var rr in RunHistory)
			{
				rs.Duration = rs.Duration.Add(rr.Duration);
				rs.Exceptions.AddRange(rr.Exceptions);
				rs.Failed += rr.Failed;
				rs.Launched += rr.Launched;
				rs.Passed += rr.Passed;
				rs.Skipped += rr.Skipped;
				rs.Unknown += rr.Unknown;
			}

			rs.RunsCount = RunHistory.Count;

			return rs;
		}

		/// <summary>
		/// Prints the aggregated counters for all runs.
		/// </summary>
		public void PrintTotalStats()
		{
			Print.AsSystemTrace("{0,-33}", "TOTAL RESULTS");
			GetTotalStats().PrintStats();
			Print.Line();
		}

		/// <summary>
		/// The test runs history in order.
		/// </summary>
		public List<RunRecord> RunHistory { get; private set; }

		/// <summary>
		/// All discovered ITestSurface implementations
		/// </summary>
		public Dictionary<string, Type> SurfaceTypes { get; private set; }

		void cmd(Dictionary<string, List<string>> argsMap)
		{
			$"SURFACE LAUNCHER (command mode)".AsSystemTrace();
			Print.Line();

			var cmdArgs = ArgsParser.Parse(DEF_OPTION_KEY, SUB_ARG_SWITCH, argsMap[CMD].ToArray());

			if (cmdArgs.ContainsKey(CMD_TAG_STATS)) tagStats();
			else "No matching command.".AsWarn();

			Print.Line();
		}

		void tagStats()
		{
			"Tag stats".Trace(Print.Command);

			var tagsMap = new Dictionary<string, int>();

			foreach (var t in SurfaceTypes)
			{
				var test = Activator.CreateInstance(t.Value) as ITestSurface;
				var tags = getTags(test);

				if (tags != null)
					foreach (var tag in tags)
						if (tagsMap.ContainsKey(tag)) tagsMap[tag]++;
						else tagsMap.Add(tag, 1);
			}

			Print.Line();

			foreach (var tk in tagsMap.OrderByDescending(x => x.Value))
				$"{tk.Key} ({tk.Value})".AsInfo();

			Print.Line();
		}

		string[] getTags(ITestSurface test)
		{
			string[] tags = null;

			if (!string.IsNullOrWhiteSpace(test.Tags))
			{
				tags = test.Tags.Split(COMMA, StringSplitOptions.RemoveEmptyEntries);

				for (int i = 0; i < tags.Length; i++)
					tags[i] = tags[i].Trim();
			}

			return tags;
		}

		void printRunHeader()
		{
			var t = typeof(SurfaceLauncher);
			var v = t.Assembly.GetName().Version;
			var vs = $"v{v.Major}.{v.Minor}.{v.Build}";
			var libs = SurfaceTypes.Select(x => x.Value.Assembly).Distinct().ToArray();
			const string pad60 = "{0,-70}";
			Print.Line();
			Print.trace(pad60, 0, false, Print.SystemTrace, "SURFACE LAUNCHER " + vs);
			Print.AsSystemTrace(pad60, "  Switches: ");
			Print.AsSystemTrace(pad60, "  +all: activates all tests ");
			Print.AsSystemTrace(pad60, "  +all -wtags  tag1 tag2: activates all tests having at least one tag match ");
			Print.AsSystemTrace(pad60, "  +all -wotags tag1 tag2: activates all tests not having any of the specified tags");
			Print.AsSystemTrace(pad60, "  +all -wxtags tag1 tag2: activates all tests having all of the specified tags");
			Print.AsSystemTrace(pad60, "  +TestSurfaceClassName: launches one test only ");
			Print.AsSystemTrace(pad60, "  +break: on first failure ");
			Print.AsSystemTrace(pad60, "  -skip: ignores the specified tests (+all -skip T1 T2)");
			Print.AsSystemTrace(pad60, "  -info: traces the test descriptions (after +T or +all)");
			Print.AsSystemTrace(pad60, "  +/-notrace: ignores the info tracing; +notrace is global");
			Print.AsSystemTrace(pad60, "  +noprint: all Print methods are ignored");
			Print.Line();
			$"Tests found: {SurfaceTypes.Count}".AsSystemTrace();
			$"Assemblies: {libs.Length}".AsSystemTrace();
			if (libs != null && libs.Length > 0)
				foreach (var a in libs)
				{
					var isDebug = a.GetCustomAttributes(false)
						.OfType<DebuggableAttribute>()
						.Any(i => i.IsJITTrackingEnabled);

					if (isDebug) $"[D] {a.FullName}".trace(2, false, ConsoleColor.Gray);
					else $"[R] {a.FullName}".trace(2, false, ConsoleColor.DarkGreen);
				}
			Print.Line();
		}

		/// <summary>
		/// Prints the test info on Start().
		/// </summary>
		public bool PrintTestInfo { get; set; } = true;

		public const string DEF_OPTION_KEY = "*";
		public const char ARG_SWITCH = '+';
		public const char SUB_ARG_SWITCH = '-';
		public readonly string CMD = ARG_SWITCH + "cmd";
		public readonly string ALL = ARG_SWITCH + "all";
		public readonly string BREAK = ARG_SWITCH + "break";
		public readonly string GLOBAL_NOTRACE = ARG_SWITCH + "notrace";
		public readonly string NO_TRACE = SUB_ARG_SWITCH + "notrace";
		public readonly string INFO = SUB_ARG_SWITCH + "info";
		public readonly string SKIP = SUB_ARG_SWITCH + "skip";
		public readonly string NO_PRINT = ARG_SWITCH + "noprint";
		public readonly string WITH_TAGS = SUB_ARG_SWITCH + "wtags";
		public readonly string WITHOUT_TAGS = SUB_ARG_SWITCH + "wotags";
		public readonly string WITH_EXACT_TAGS = SUB_ARG_SWITCH + "wxtags";
		public readonly string CMD_TAG_STATS = SUB_ARG_SWITCH + "tagstats";
		readonly string[] COMMA = new string[] { "," };



		int isRuning;
	}
}
