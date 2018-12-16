using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRunner
{
	public class Runner
	{
		public void Run(string testLibName, string[] args)
		{
			Print.AsSystemTrace("| ======================================");
			Print.AsSystemTrace("| Test runner for {0}:", testLibName);
			Print.AsSystemTrace("| To run all tests use the -all switch; for specific surface class -ITestSurface_ClassName");
			Print.AsSystemTrace("| To suppress info tracing add -notrace");
			Print.AsSystemTrace("| For help pass the -info argument along with either -all or -YourTestClass");
			Print.AsSystemTrace("| ======================================");

			Console.WriteLine();

			var argsMap = new ArgsParser(args).Map;
			var SurfaceTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => typeof(ITestSurface).IsAssignableFrom(p) && p.IsClass)
				.ToList();

			if (argsMap.ContainsKey("-notrace")) Print.IgnoreInfo = true;

			Print.AsSystemTrace("There are {0} tests available", SurfaceTypes.Count);
			Console.WriteLine();

			ITestSurface test = null;
			int passed = 0, failed = 0, requireargs = 0, unknowns = 0, launched = 0;

			foreach (var st in SurfaceTypes)
				try
				{
					var runAll = argsMap.ContainsKey("-all");
					if (runAll || argsMap.ContainsKey(string.Format("-{0}", st.Name)))
					{
						test = Activator.CreateInstance(st) as ITestSurface;
						if (test != null)
						{
							if (runAll && test.RequireArgs)
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

								// Copy because each test may add switches to reuse functionality.
								// For example if -all the Run will add default flags, which should
								// be private for that specific test only.
								var argsMapCopy = new Dictionary<string, List<string>>(argsMap);
								var started = DateTime.Now;
								test.Run(argsMapCopy).Wait();
								var dur = DateTime.Now.Subtract(started);
								var duratoin = string.Format("[{0}m {1}s {2}ms]", dur.Minutes, dur.Seconds, dur.Milliseconds);
								launched++;
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
				}
				catch (Exception ex)
				{
					Print.AsError(ex.ToString());
				}

			var lines = FormatText.JoinLines(
				"Results:",
				"  Total tests: " + SurfaceTypes.Count,
				"  Launched: " + launched,
				"  Passed: " + passed,
				"  Failed: " + failed,
				"  Unknown: " + unknowns,
				"  Skipped (require args): " + requireargs
				);

			Print.Trace(lines, ConsoleColor.White, ConsoleColor.DarkBlue, SurfaceTypes.Count, passed, failed, unknowns, requireargs);
		}
	}
}
