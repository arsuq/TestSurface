using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestRunner
{
	using ArgMap = IDictionary<string, List<string>>;

	public interface ITestSurface
	{
		/// <summary>
		/// Description of the test and the arguments.
		/// </summary>
		string Info { get; }

		/// <summary>
		/// Set if the test fails.
		/// </summary>
		string FailureMessage { get; }

		/// <summary>
		/// Set in all cases, null means unknown outcome.
		/// </summary>
		bool? Passed { get; }

		/// <summary>
		/// Indicates whether the whole test was covered or some parts were skipped.
		/// </summary>
		bool IsComplete { get; }

		/// <summary>
		/// Some tests have no default initial values or require multiple instances
		/// of the test app to be running at the sane time.
		/// </summary>
		bool RequiresArgs { get; }
		
		/// <summary>
		/// A task which may or may not be started.
		/// The runner will Wait() for each task.
		/// </summary>
		/// <param name="args">The parsed arguments as a dictionary where keys are the switches.</param>
		/// <returns>A task to be awaited.</returns>
		Task Run(ArgMap args);
	}
}
