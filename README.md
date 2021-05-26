
![](TestSurface.png)

# Test Surface

[![.NET](https://github.com/arsuq/TestSurface/actions/workflows/dotnet.yml/badge.svg)](https://github.com/arsuq/TestSurface/actions/workflows/dotnet.yml)

## Description

The lib defines a test contract, provides a command line arguments parser, a simple printing utility, 
a recursive object comparer and a test launcher. 

```csharp
public interface ITestSurface
{
    string Info { get; }   // The test description
    string Tags { get; }   // A comma-separated list of tags 
    string FailureMessage { get; }
    bool? Passed { get; }  
    bool IsComplete { get; }   // Useful for multi-method tests
    bool IndependentLaunchOnly { get; }  // If true, the test can't be started with +all
    Task Start(ArgMap args);
}
``` 

The launcher can be started in two modes:

- with specific *ITestSurface* implementations: ``` +ITestSurfaceImplName -options o1 o2 +ITestSurfaceImplName2```
- with the **+all** switch to discover and activate all compatible types. Tests having *IndependentLaunchOnly = true* will be 
  ignored when *+all* is present.

> Note: All implementations must have a default constructor.

### Arguments

Each test is provided with its own subset of the original arguments or with a map with *"+all"* key.
The map keys are the switches with the prefix (e.g. +all, -option) and the values are the arguments following the switch.
Values with no leading switch are added in a list with a "*" key. For example with
``` launcher.Start("+TS","nolead", "-leadOption", "value") ``` the test will receive a map with two keys
```leadOption [value]``` and ```* [nolead] ```.  

In code pass each argument as a separate string e.g.
```launcher.Start("+TS1", "-option", "option with spaces", "o2", "+TS2"); ```

### SurfaceLauncher options

- including **-info** will take the Info property and trace it instead of executing the Start method.   
  Launching with ```+all -info```  will trace all test descriptions.
- with **+/-notrace** all *Print.AsInfo()* or *Print.Trace()* calls will be ignored. The +notrace is global for all tests.
- **+noprint** disables all Print methods, including the test launcher status info. It's equivalent to ```Print.IgnoreAll = true```
- **+break** stops the launcher on the first failure
- **-skip** followed by target names will ignore them if **+all** is present: ```+all -skip T1 T2```
- **-wtags** followed by a list of tags launches all tests having at least one matching tag
- **-wotags** starts the tests which don't have any tag in common with the args
- **-wxtags** starts the tests having all of the provided tags

 Only one of the tag switches can be applied, and it must be as a sub-switch of *+all*: ```+all -wtags tag1 tag2 ```
 
 To list the tests with their descriptions: ```+all -wtags tag1 tag2 -info```

- **+cmd** with a sub-option executes a command
 
 For example ``` +cmd -tagstats``` prints the tags and the number of tests they are declared in 


### Assert

The	```Assert.SameValues(object , object b, BindingFlags bf) ``` compares two objects by-value 
in depth using reflection. This is useful for template comparison, i.e. setting up an object tree as
a passing condition and comparing it with a runner instance at the end of the test. If no BindingFlags
are provided only the public members are observed. 

> Note that collections and enumerations are compared recursively for each item.

The types are reflected once and kept in a static cache, which can be cleared with ````Assert.ClearTypeCache()````.

```csharp
public Task Start(IDictionary<string, List<string>> args)
{
    var model = new
    {
        thisMustBeTrue = false,
        thisSquenceIsSuperImportant = new double[] { 1.012, 0.001, 3.912 },
        innerObj = new { text = "whatever" }
    };

    var comp = new
    {
        thisMustBeTrue = false,
        thisSquenceIsSuperImportant = new double[] { 1.0212, 0.001, 3.912 },
        innerObj = new { text = "whatever" }
    };

    // Test...

    Passed = Assert.SameValues(model, comp);

    return Task.CompletedTask;
}
```

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

The SurfaceLauncher keeps records of the activated test types, their input arguments and unhandled exceptions.
One could inspect a specific test from the ```SurfaceRunRecord``` instance:

```csharp
var r = new SurfaceLauncher();

r.Start(args1);
r.Start(args2);

var rr = r.RunHistory[runIndex];     // The RunRecord has the run stats
var sr = rr.Tests[testType];         // A SurfaceRunRecord
var test = (TheTestType)sr.Instance; // The activated test

// sr.ArgsMap is a reference to the input map
// sr.Exception is either unhandled or re-thrown by the test.Start

var ts = run.GetTotalStats(); // Aggregates all stats   

```





## Usage

Add a reference to the TestRunner.dll and implement the ITestSurface interface:
```csharp
public class XYZSurface : ITestSurface
{
    public string Info => "Test description...";
    public string Tags => "tag1, tag2";
    public string FailureMessage { get; private set; }
    public bool? Passed { get; private set; }
    public bool IndependentLaunchOnly => false;
    public bool IsComplete { get; private set; }
	
    public async Task Start(Dictionary<string, List<string>> args)
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


Start new SurfaceLauncher instance:


```csharp
using System;
using TestSurface;

namespace Tests
{
    class Program
    {
        static int Main(string[] args)
        {
            var l = new SurfaceLauncher();
           
            // (1) Relay the terminal
            // The args should contain +all or +ITestSurfaceTypeName
            l.Start(args);

            // (2) Or launch specific tests from code
            // Pass each argument as a separate string to preserve spaces 
            l.Start("+TS1", "-option", "with space", "o2", "+TS2");
            l.Start("+TS1", "-option", "o3", "o4");
            
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

