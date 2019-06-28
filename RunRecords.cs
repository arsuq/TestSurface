using System;
using System.Collections.Generic;

namespace TestSurface
{
	/// <summary>
	/// The basic Start counters. When returned from GetTotalStats()
	/// the values are aggregated from all runs.
	/// </summary>
	public class RunRecordStats
	{
		public RunRecordStats() { Exceptions = new List<Exception>(); }

		public int RunsCount { get; internal set; }
		public int Passed { get; internal set; }
		public int Failed { get; internal set; }
		public int Skipped { get; internal set; }
		public int Unknown { get; internal set; }
		public int Launched { get; internal set; }
		public TimeSpan Duration { get; internal set; }
		public List<Exception> Exceptions { get; internal set; }

		public void PrintStats()
		{
			var rdurStr = $"{Duration.Hours}h {Duration.Minutes}m {Duration.Seconds}s {Duration.Milliseconds}ms ";
			const string pad = "  {0, 13} {1, -17}";

			Print.AsSystemTrace(pad, "Launched:", Launched);
			Print.AsSystemTrace(pad, "Passed:", Passed);
			Print.AsSystemTrace(pad, "Failed:", Failed);
			Print.AsSystemTrace(pad, "Unknown:", Unknown);
			Print.AsSystemTrace(pad, "Skipped:", Skipped);
			Print.AsSystemTrace(pad, "Runs count:", RunsCount);
			Print.AsSystemTrace(pad, "Duration:", rdurStr);
		}
	}

	/// <summary>
	/// Keeps a map of the activated ITestSurface types
	/// </summary>
	public class RunRecord : RunRecordStats
	{
		public RunRecord(int runIdx, string[] origArgs)
		{
			RunsCount = runIdx;
			Tests = new Dictionary<Type, SurfaceRunRecord>();
			Exceptions = new List<Exception>();
			OriginalArgs = origArgs;
		}

		/// <summary>
		/// As passed to the launcher
		/// </summary>
		public string[] OriginalArgs { get; internal set; }

		/// <summary>
		/// A map of the surface instances. 
		/// </summary>
		public Dictionary<Type, SurfaceRunRecord> Tests { get; internal set; }
	}

	/// <summary>
	/// An ITestSurface instance run record
	/// </summary>
	public class SurfaceRunRecord
	{
		public SurfaceRunRecord(ITestSurface surf, int runIdx) : this(surf, runIdx, new Dictionary<string, List<string>>(), null) { }
		public SurfaceRunRecord(ITestSurface surf, int runIdx, Dictionary<string, List<string>> args, Exception ex)
		{
			RunIndex = runIdx;
			Instance = surf;
			ArgsMap = args;
			Exception = ex;
		}

		/// <summary>
		/// Index in the runs sequence.
		/// </summary>
		public int RunIndex { get; internal set; }

		/// <summary>
		/// The Surface instance.
		/// </summary>
		public ITestSurface Instance { get; internal set; }

		/// <summary>
		/// A copy of the original args, including private modification.
		/// </summary>
		public Dictionary<string, List<string>> ArgsMap { get; internal set; }

		/// <summary>
		/// Caught from the launcher.
		/// </summary>
		public Exception Exception { get; internal set; }

		/// <summary>
		/// The running time.
		/// </summary>
		public TimeSpan Duration { get; internal set; }
	}
}
