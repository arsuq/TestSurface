using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRunner
{
	public class Runner
	{
		public void Run(string[] args)
		{
			Print.AsWarn("This is the MemoryLanes test runner.");
			Print.AsWarn("To run all tests use the -all switch; for specific surface class -ITestSurface_ClassName");
			Print.AsWarn("To suppress info tracing add -notrace");
			Print.AsWarn("For help pass the -info argument along with either -all or -YourTestClass");

			var argsMap = new ArgsParser(args).Map;
			var SurfaceTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => typeof(ITestSurface).IsAssignableFrom(p) && p.IsClass)
				.ToList();

			if (argsMap.ContainsKey("-notrace")) Print.IgnoreInfo = true;

			ITestSurface test = null;

			foreach (var st in SurfaceTypes)
				try
				{
					if (argsMap.ContainsKey("-all") || argsMap.ContainsKey(string.Format("-{0}", st.Name)))
					{
						test = Activator.CreateInstance(st) as ITestSurface;
						if (test != null && !test.RequireArgs)
						{
							if (argsMap.ContainsKey("-info"))
							{
								Print.AsSysyemTrace(string.Format("{0}:", st.Name));
								Print.AsHelp(test.Info());
								Console.WriteLine();
							}
							else
							{
								Print.AsSysyemTrace(string.Format("{0}:", st.Name));
								test.Run(argsMap).Wait();
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
						Print.AsSysyemTrace("The test info:");
						Print.AsHelp(test.Info());
					}
				}
				catch (Exception ex)
				{
					Print.AsError(ex.ToString());
				}
		}
	}
}
