using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestRunner
{
	using ArgMap = IDictionary<string, List<string>>;

	public interface ITestSurface
	{
		string FailureMessage { get; }
		bool? Passed { get; }

		/// <summary>
		/// Some tests have no default initial values or require multiple instances
		/// of the test app to be running at the sane time such as the SocketLoad client-server test.
		/// </summary>
		bool RequireArgs { get; }

		/// <summary>
		/// Description of the test and the arguments.
		/// </summary>
		/// <returns>The info text.</returns>
		string Info { get; }

		/// <summary>
		/// A task which may or may not be started.
		/// The runner will Wait() for each task.
		/// </summary>
		/// <param name="args">The parsed arguments as a dictionary where keys are the switches.</param>
		/// <returns>A task to be awaited.</returns>
		Task Run(ArgMap args);
	}
}
