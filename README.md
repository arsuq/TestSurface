# Test runner

> v1.0

## Description

This lib defines an interface, command line arguments parser and a simple printing utilities, as well as a
test discovery and runner. In order to use the runner one has to implement the ITestSurface interface:

```csharp
public interface ITestSurface
{
	string Info { get; }
	string FailureMessage { get; }
	bool? Passed { get; }
	bool IsComplete { get; }
	bool RequiresArgs { get; }
	Task Run(ArgMap args);
}
``` 

The runner can be launched in two modes:

- with a specific test class and arguments, by passing a **-TheTestClassName** as a command line parameter
- the **-all** will discover all the compatible classes and invoke their Run method;
  classes having RequiresArgs = true will be ignored in this mode as they cannot be fed with specific
  data because that may brake the rest of the tests. 

The ArgMap type is in fact an IDictionary<string, List<string>> instance, produced by the ArgsParser. 
Inside the Run method each test is provided with a copy of the original dictionary, so that common test paths 
could be reused by adding arguments safely just for the specific test instance. For example handling  
the -all mode may be accomplished by inserting default switches and values at the beginning of the Run method
and then continuing with a common execution path.

The IDictionary<string, List<string>> holds the switches as keys (with the dash, e.g. -all) and a list of
string arguments, provided in the console after the switch.

Additional arguments:

- including **-info** will take the Info property and trace it instead of executing the Run method.
- with **-notrace** all *Print.Info()*, *Print.Trace()* invocations will be ignored
- use **-brake** to stop the test launcher on the first failure

Launching with -all -info will trace all test descriptions. 

Use **Print** to trace info during the test instead of Console directly for it can be suppressed
with -notrace and also the Print.Trace() method has a lock for correct coloring in multi-threaded code. 



