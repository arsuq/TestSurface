using System;
using System.Collections.Generic;
using System.Linq;

namespace TestSurface
{
	public class Runner
	{
		public void Run(string[] args)
		{
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
			int passed = 0, failed = 0, requireargs = 0, unknowns = 0, launched = 0;

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
							if (runAll && test.RequiresArgs)
							{
								requireargs++;
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
								Console.WriteLine();
								Print.AsTestHeader(string.Format("{0}:", st.Name));
								launched++;
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
										passed++;
										Print.AsTestSuccess(string.Format("OK: {0} {1}", st.Name, duratoin));
									}
									else
									{
										failed++;
										Print.AsTestFailure(string.Format("FAIL: {0} {1} Ex: {2}", st.Name, duratoin, test.FailureMessage));
										if (breakOnFirstFailure) break;
									}
								}
								else
								{
									unknowns++;
									Print.AsTestUnknown(string.Format("UNKNOWN: {0} {1}", st.Name, duratoin));
								}
							}
						}
						else Print.AsTestFailure(string.Format("Failed to activate: {0}", st.Name));
					}
				}
				catch (KeyNotFoundException kex)
				{
					Print.AsError("Argument error for test: " + st.Name);
					Print.AsError(kex.Message);
					if (test != null)
					{
						Print.AsSystemTrace("The test info:");
						Print.AsHelp(test.Info);
					}
					failed++;
				}
				catch (Exception ex)
				{
					failed++;
					Print.AsError(ex.ToString());
				}


			Console.WriteLine();
			const string pad12 = "  {0, 12} {1,-5}";
			Print.AsSystemTrace("{0, -20}", "RESULTS:");
			Print.AsSystemTrace(pad12, "Tests found:", SurfaceTypes.Count);
			Print.AsSystemTrace(pad12, "Launched:", launched);
			Print.AsSystemTrace(pad12, "Passed:", passed);
			Print.AsSystemTrace(pad12, "Failed:", failed);
			Print.AsSystemTrace(pad12, "Unknown:", unknowns);
			Print.AsSystemTrace(pad12, "Skipped:", requireargs);

			Console.WriteLine();

			if (launched < 1 && !argsMap.ContainsKey("-info"))
				Print.Trace("Did you forget specifying a test target? Add -all or -TestSurface as an argument.", ConsoleColor.Red, ConsoleColor.Black);
		}
	}
}
