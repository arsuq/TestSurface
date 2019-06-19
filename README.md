
![](TestSurface.png)

# Test Surface

[![Build status](https://ci.appveyor.com/api/projects/status/744v953l9v35b05a?svg=true)](https://ci.appveyor.com/project/arsuq/files-5y6ur)
> [v1.1.4 changelog](CHANGELOG.md)

## Description

The lib defines a testing contract, provides a command line arguments parser, a simple printing utility
and a test runner. In order to use the runner one has to implement the ITestSurface interface:

```csharp
public interface ITestSurface
{
	string Info { get; }
	string FailureMessage { get; }
	bool? Passed { get; }
	bool IsComplete { get; }
	bool IndependentLaunchOnly { get; }
	Task Run(ArgMap args);
}
``` 

The runner can be launched in two modes:

- with specific *ITestSurface* implementations: ``` +ITestSurfaceImplName -options o1 o2 +ITestSurfaceImplName2```
- with **+all** to discover and activate all compatible types. Tests having *IndependentLaunchOnly = true* will be 
  ignored when *+all* is present.

> Note: All implementations must have a default constructor.

### Arguments

Each test is provided with its subset of the original arguments or with a map with *"+all"* key.
The map keys are the switches with the prefix (e.g. +all, -option) and the values are the arguments following the switch.
Values with no leading switch are added in a list with a "*" key. For example with
``` runner.Run("+TS","nolead", "-leadOption", "value") ``` the test will receive a map with two keys
```leadOption [value]``` and ```* [nolead] ```.  



### Launcher options

- including **-info** will take the Info property and trace it instead of executing the Run method.   
  Launching with ```+all -info```  will trace all test descriptions.
- with **+/-notrace** all *Print.AsInfo()* or *Print.Trace()* calls will be ignored. The +notrace is global for all tests.
- **+noprint** disables all Print methods, including the test launcher status info. It's equivalent to ```Print.IgnoreAll = true```
- **+break** stops the launcher on the first failure
- **-skip** followed by target names will ignore them if **+all** is present: ```+all -skip T1 T2```

 In code pass each argument as a separate string e.g.
```runner.Run("+TS1", "-option", "option with spaces", "o2", "+TS2"); ```


### Print 

Use *Print* to trace info during the test instead of the Console directly for it can be suppressed
with **-notrace** or **-noprint**. Additionally, all Print methods are synchronized for usage in multi-threaded code 
if ```Print.SerializeTraces = true```, which is the default value. 
When no printing is needed ```Print.IgnoreAll = false``` will disable it.

**Note:**   
Print will drop traces if awaits more than ```LockAwaitMS ``` and will throw a *TimeoutException* if 
```Print.ThrowOnLockTimeout``` is enabled (false by default). If that behavior is not acceptable 
one should set ```Print.SerializeTraces = false``` and apply external synchronization or use the Console directly.


### Records

The Runner keeps records of the activated test types, their input arguments and unhandled exceptions.
One could inspect a specific test instance from the ```SurfaceRunRecord``` instance:

```csharp
var r = new Runner();

r.Run(args1);
r.Run(args2);

var rr = r.RunHistory[runIndex];     // The RunRecord has the run stats
var sr = rr.Tests[testType];         // A SurfaceRunRecord
var test = (TheTestType)sr.Instance; // The activated test

// sr.ArgsMap is a reference to the input map
// sr.Exception is not handled or re-thrown by the test.Run

var ts = run.GetTotalStats(); // Aggregates all stats   

```





## Usage

Add a reference to the TestRunner.dll and implement the ITestSurface interface:
```csharp
public class XYZSurface : ITestSurface
{
    public string Info => "Test description...";
    public string FailureMessage { get; private set; }
    public bool? Passed { get; private set; }
    public bool IndependentLaunchOnly => false;
    public bool IsComplete { get; private set; }
	
    // Add members
    public KnownException KnownEx { get; private set; }  	

    public async Task Run(Dictionary<string, List<string>> args)
    {
        try
        {
            // Add +all support  
            if (args.ContainsKey("+all"))
                args.Add("-param", new List<string>() { "10", "1000" });

            // The common path, i.e. either +all or +XYZSurface
            var P = args["-param"];

            // Check for default values  
            if (args.ContainsKey("*")){}

            // Assert...
			
            if (!Passed.HasValue) Passed = true;
            IsComplete = true;
        }
        catch(KnownException x)
        {
            KnownEx = x;
            Passed = false;
            FailureMessage = x.Message;
        }
        catch(Exception ex)
        {
            // Not handling or re-throwing an exception will preserve it
            // in the SurfaceRunRecord.Exception property.
        }
    }
}
```


Launch a new Runner instance:


```csharp
using System;
using TestSurface;

namespace Tests
{
    class Program
    {
        static int Main(string[] args)
        {
            var r = new Runner();
           
            // (1) Relay the terminal
            // The args should contain +all or +ITestSurfaceTypeName
            r.Run(args);

            // (2) Or launch specific tests from code
            // Pass each argument as a separate string to preserve spaces 
            r.Run("+TS1", "-option", "with space", "o2", "+TS2");
            r.Run("+TS1", "-option", "o3", "o4");
            
            // Check the Results
            var rs = r.GetTotalStats();
			
            // Inspect specific instance
            var ts = (TS1)r.RunHistory[1].Tests[typeof(TS1)].Instance;
			
            // Get the total failures count
            return rs.Failed > 0 ? -rs.Failed : 0;
        }
    }
}
```

