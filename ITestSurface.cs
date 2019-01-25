using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestSurface
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
		/// of the test application to be running at the same time.
		/// </summary>
		bool IndependentLaunchOnly { get; }
		
		/// <summary>
		/// If launched, the runner will wait for it to complete.
		/// </summary>
		/// <param name="args">The parsed arguments dictionary where the keys are switches.</param>
		/// <returns>A task to be awaited.</returns>
		Task Run(ArgMap args);
	}
}
