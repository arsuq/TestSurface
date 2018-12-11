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

			foreach (var st in SurfaceTypes)
				try
				{
					var runAll = argsMap.ContainsKey("-all");
					if (runAll || argsMap.ContainsKey(string.Format("-{0}", st.Name)))
					{
						test = Activator.CreateInstance(st) as ITestSurface;
						if (test != null)
						{
							if (runAll && test.RequireArgs) continue;

							if (argsMap.ContainsKey("-info"))
							{
								Print.AsTestHeader(string.Format("{0}:", st.Name));
								Print.AsHelp(test.Info);
								Console.WriteLine();
							}
							else
							{
								Print.AsTestHeader(string.Format("{0}:", st.Name));

								// Copy because each test may add switches to reuse functionality.
								// For example if -all the Run will add default flags, which should
								// be private for that specific test only.
								var argsMapCopy = new Dictionary<string, List<string>>(argsMap);

								test.Run(argsMapCopy).Wait();
								if (test.Passed.HasValue)
								{
									if (test.Passed.Value) Print.AsTestSuccess(string.Format("OK: {0}", st.Name));
									else Print.AsTestFailure(string.Format("FAIL: {0} Ex: {1}", st.Name, test.FailureMessage));
								}
								else Print.AsTestUnknown(string.Format("UNKNOWN: {0} ", st.Name));
							}
						}
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
		}
	}
}
