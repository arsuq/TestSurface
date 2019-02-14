using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TestSurface
{
	public class Runner
	{
		public Runner()
		{
			Exceptions = new List<Exception>();
			ResultMap = new Dictionary<Type, ITestSurface>();
		}

		/// <summary>
		/// Starts all discoverable tests. 
		/// </summary>
		/// <remarks>
		/// Only one Run execution at a time is allowed.
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">If another Run is still executing.</exception>
		/// <param name="args">The arguments list</param>
		public void Run(string[] args)
		{
			if (Interlocked.CompareExchange(ref isRuning, 1, 0) > 0) throw new InvalidOperationException("Tests are running");

			RunsCount++;
			Args = args;
			Exceptions.Clear();
			ResultMap.Clear();

			const string pad60 = "{0,-60}";
			Print.AsSystemTrace(pad60, "TEST RUNNER");
			Print.AsSystemTrace(pad60, "  Switches: ");
			Print.AsSystemTrace(pad60, "  -all: runs all tests ");
			Print.AsSystemTrace(pad60, "  -TheTestSurfaceClassName: launches one test only ");
			Print.AsSystemTrace(pad60, "  -notrace: ignores info tracing ");
			Print.AsSystemTrace(pad60, "  -break: on first failure ");
			Print.AsSystemTrace(pad60, "  -info: traces the test descriptions  ");

			Console.WriteLine();

			var argsMap = new ArgsParser(args).Map;
			var SurfaceTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => typeof(ITestSurface).IsAssignableFrom(p) && p.IsClass)
				.ToList();

			if (argsMap.ContainsKey("-notrace")) Print.IgnoreInfo = true;

			Print.AsSystemTrace($"There are {SurfaceTypes.Count} tests.");
			Console.WriteLine();

			ITestSurface test = null;
			Passed = 0; Failed = 0; Skipped = 0; Unknown = 0; Launched = 0;
			var runStart = DateTime.Now;

			foreach (var st in SurfaceTypes)
				try
				{
					var runAll = argsMap.ContainsKey("-all");
					var breakOnFirstFailure = argsMap.ContainsKey("-break");

					if (runAll || argsMap.ContainsKey(string.Format("-{0}", st.Name)))
					{
						test = Activator.CreateInstance(st) as ITestSurface;

						if (test != null)
						{
							if (runAll && test.IndependentLaunchOnly)
							{
								Skipped++;
								continue;
							}

							if (argsMap.ContainsKey("-info"))
							{
								Print.AsTestHeader(string.Format("{0}:", st.Name));
								Print.AsHelp(test.Info);
								Console.WriteLine();
							}
							else
							{
								ResultMap.Add(st, test);

								Console.WriteLine();
								Print.AsTestHeader(string.Format("{0}:", st.Name));
								Launched++;
								// Copy because each test may add switches to reuse functionality.
								// For example if -all the Run will add default flags, which should
								// be private for that specific test only.
								var argsMapCopy = new Dictionary<string, List<string>>(argsMap);
								var started = DateTime.Now;
								test.Run(argsMapCopy).Wait();
								var dur = DateTime.Now.Subtract(started);
								var duratoin = string.Format("[{0}m {1}s {2}ms]", dur.Minutes, dur.Seconds, dur.Milliseconds);

								if (test.Passed.HasValue)
								{
									if (test.Passed.Value)
									{
										Passed++;
										Print.AsTestSuccess(string.Format("OK: {0} {1}", st.Name, duratoin));
									}
									else
									{
										Failed++;
										Print.AsTestFailure(string.Format("FAIL: {0} {1} Ex: {2}", st.Name, duratoin, test.FailureMessage));
										if (breakOnFirstFailure) break;
									}
								}
								else
								{
									Unknown++;
									Print.AsTestUnknown(string.Format("UNKNOWN: {0} {1}", st.Name, duratoin));
								}
							}
						}
						else Print.AsTestFailure(string.Format("Failed to activate: {0}", st.Name));
					}
				}
				catch (KeyNotFoundException kex)
				{
					Exceptions.Add(kex);
					Print.AsError("Argument error for test: " + st.Name);
					Print.AsError(kex.Message);
					if (test != null)
					{
						Print.AsSystemTrace("The test info:");
						Print.AsHelp(test.Info);
					}
					Failed++;
				}
				catch (Exception ex)
				{
					Exceptions.Add(ex);
					Failed++;
					Print.AsError(ex.ToString());
				}

			var rdur = DateTime.Now.Subtract(runStart);
			var rdurStr = $"{rdur.Hours}h {rdur.Minutes}m {rdur.Seconds}s {rdur.Milliseconds}ms ";

			Console.WriteLine();
			const string pad = "  {0, 13} {1,-5}";
			Print.AsSystemTrace("{0, -21}", "RESULTS:");
			Print.AsSystemTrace(pad, "Tests found:", SurfaceTypes.Count);
			Print.AsSystemTrace(pad, "Runs count:", RunsCount);
			Print.AsSystemTrace(pad, "Launched:", Launched);
			Print.AsSystemTrace(pad, "Passed:", Passed);
			Print.AsSystemTrace(pad, "Failed:", Failed);
			Print.AsSystemTrace(pad, "Unknown:", Unknown);
			Print.AsSystemTrace(pad, "Skipped:", Skipped);
			Print.AsSystemTrace(pad, "Run duration:", rdurStr );

			Console.WriteLine();

			if (Launched < 1 && !argsMap.ContainsKey("-info"))
				Print.Trace("Did you forget specifying a test target? Add -all or -TestSurface as an argument.", ConsoleColor.Red, ConsoleColor.Black);

			Interlocked.Exchange(ref isRuning, 0);
		}

		public int Passed { get; private set; }
		public int Failed { get; private set; }
		public int Skipped { get; private set; }
		public int Unknown { get; private set; }
		public int Launched { get; private set; }
		public int RunsCount { get; private set; }

		public string[] Args { get; private set; }
		public Dictionary<Type, ITestSurface> ResultMap { get; private set; }
		public List<Exception> Exceptions { get; private set; }

		int isRuning;
	}
}
