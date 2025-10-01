# TestSurface: Simple Testing Framework

![TestSurface](TestSurface.png)

[![.NET](https://github.com/arsuq/TestSurface/actions/workflows/dotnet.yml/badge.svg)](https://github.com/arsuq/TestSurface/actions/workflows/dotnet.yml)

> **Simple testing framework with one interface, deep object comparison, and command-line control.**

## 🎯 Why TestSurface Exists

If you're reading this, chances are you've experienced one (or all) of these frustrations:

- **⚙️ Setup Paralysis**: Spending hours configuring test frameworks instead of writing tests
- **🏷️ Attribute Hell**: Decorating every method with `[Test]`, `[Fact]`, `[TestMethod]`, `[Setup]`, `[TearDown]`...
- **🎭 Magic Dependencies**: Your tests break because some invisible framework magic changed
- **🔧 Integration Nightmares**: Different test types require different runners, different configurations

**TestSurface was born from a simple belief: Testing should amplify your productivity, not drain it.**

## 📋 Description

The lib defines a test contract, provides a command line arguments parser, a simple printing utility, 
a recursive object comparer and a test launcher.

```csharp
public interface ITestSurface
{
    string Info { get; }                    // The test description
    string Tags { get; }                    // A comma-separated list of tags 
    string FailureMessage { get; }          // Error details when failed
    bool? Passed { get; }                   // null=unknown, true/false=result
    bool IsComplete { get; }                // Useful for multi-method tests
    bool IndependentLaunchOnly { get; }     // If true, the test can't be started with +all
    Task Start(ArgMap args);                // Test execution
}
```

## 🚀 Quick Start

The launcher can be started in two modes:

- **Specific tests**: `+ITestSurfaceImplName -options o1 o2 +ITestSurfaceImplName2`
- **All tests**: `+all` switch to discover and activate all compatible types

> 📝 **Note**: All implementations must have a default constructor.

```csharp
public class MyTest : ITestSurface
{
    public string Info => "What this test does";
    public string Tags => "unit, fast";
    public string FailureMessage { get; private set; }
    public bool? Passed { get; private set; }
    public bool IsComplete => true;
    public bool IndependentLaunchOnly => false;

    public async Task Start(IDictionary<string, List<string>> args)
    {
        try
        {
            // Your test logic here
            var result = SomeOperation();
            
            Passed = result == expectedValue;
            if (!Passed.Value)
                FailureMessage = $"Expected {expectedValue}, got {result}";
        }
        catch (Exception ex)
        {
            Passed = false;
            FailureMessage = ex.Message;
        }
    }
}
```

**Run tests:**
```csharp
var launcher = new SurfaceLauncher();
launcher.Start("+all");     // All tests
launcher.Start("+MyTest");  // Specific test
```

## ⭐ Key Features

### 🔍 1. Deep Object Comparison
Compare complex objects recursively without writing assertion code:

```csharp
var expected = new Order 
{ 
    Id = 123, 
    Items = new[] { new Item { Name = "Widget", Price = 10.50m } }
};
var actual = GetOrder();

// This compares everything: fields, properties, collections, nested objects
Passed = Assert.SameValues(expected, actual);
```

### 💻 2. Command-Line Control
```bash
# Run all tests
MyApp.exe +all

# Run tests by tags
MyApp.exe +all -wtags integration
MyApp.exe +all -wotags slow

# Run specific tests with parameters
MyApp.exe +DatabaseTest -connectionString "Server=test"

# List available tests
MyApp.exe +all -info

# Stop on first failure
MyApp.exe +all +break
```

### 🎨 3. Flexible Output
```csharp
"Operation completed".AsSuccess();  // 🟢 Green
"Warning message".AsWarn();         // 🟡 Yellow  
"Error occurred".AsError();         // 🔴 Red
"Debug info".AsInfo();              // ℹ️ Default color
```

## 📚 Arguments

Each test is provided with its own subset of the original arguments or with a map with `"+all"` key.
The map keys are the switches with the prefix (e.g. +all, -option) and the values are the arguments following the switch.
Values with no leading switch are added in a list with a `"*"` key.

**Example**: `launcher.Start("+TS","nolead", "-leadOption", "value")` 
- Test receives: `leadOption [value]` and `* [nolead]`

**💡 In code**: Pass each argument as a separate string:
```csharp
launcher.Start("+TS1", "-option", "option with spaces", "o2", "+TS2");
```

## ⚙️ SurfaceLauncher Options

- **📋 `-info`**: Take the Info property and trace it instead of executing the Start method
- **🔇 `+/-notrace`**: All `Print.AsInfo()` or `Print.Trace()` calls will be ignored
- **🚫 `+noprint`**: Disables all Print methods (equivalent to `Print.IgnoreAll = true`)
- **⛔ `+break`**: Stops the launcher on the first failure
- **⏭️ `-skip`**: Ignore specified targets when `+all` is present: `+all -skip T1 T2`

### 🏷️ Tag Filtering
- **🎯 `-wtags`**: Launch tests having at least one matching tag
- **❌ `-wotags`**: Start tests which don't have any tag in common with the args  
- **✅ `-wxtags`**: Start tests having all of the provided tags

**Usage**: `+all -wtags tag1 tag2`

### 📊 Command Mode
- **📈 `+cmd -tagstats`**: Prints the tags and the number of tests they are declared in

## 🧪 Assert - Deep Object Comparison

The `Assert.SameValues(object a, object b, BindingFlags bf)` compares two objects by-value 
in depth using reflection. Perfect for template comparison and complex object verification.

**Features:**
- **🔄 Recursive**: Collections and enumerations are compared recursively for each item
- **⚡ Performance**: Types are reflected once and kept in a static cache
- **🧹 Memory**: Cache can be cleared with `Assert.ClearTypeCache()`

```csharp
public Task Start(IDictionary<string, List<string>> args)
{
    var model = new
    {
        thisMustBeTrue = false,
        thisSequenceIsSuperImportant = new double[] { 1.012, 0.001, 3.912 },
        innerObj = new { text = "whatever" }
    };

    var comp = new
    {
        thisMustBeTrue = false,
        thisSequenceIsSuperImportant = new double[] { 1.0212, 0.001, 3.912 },
        innerObj = new { text = "whatever" }
    };

    // Deep comparison happens here
    Passed = Assert.SameValues(model, comp);

    return Task.CompletedTask;
}
```

## 🖨️ Print System

Use `Print` to trace info during the test instead of Console directly - it can be suppressed with **-notrace** or **-noprint**.

**Features:**
- **🔒 Thread-Safe**: All Print methods are synchronized when `Print.SerializeTraces = true` (default)
- **⏱️ Timeout Control**: Will drop traces if awaits more than `LockAwaitMS`
- **⚠️ Exception Handling**: Throws `TimeoutException` if `Print.ThrowOnLockTimeout` is enabled

**💡 Alternative**: Set `Print.SerializeTraces = false` and apply external synchronization or use Console directly.

## 📊 Records & Execution History

The SurfaceLauncher keeps records of the activated test types, their input arguments and unhandled exceptions.

```csharp
var launcher = new SurfaceLauncher();

launcher.Start(args1);
launcher.Start(args2);

var runRecord = launcher.RunHistory[runIndex];     // The RunRecord has the run stats
var surfaceRecord = runRecord.Tests[testType];     // A SurfaceRunRecord
var testInstance = (TheTestType)surfaceRecord.Instance; // The activated test

// surfaceRecord.ArgsMap is a reference to the input map
// surfaceRecord.Exception is either unhandled or re-thrown by the test.Start

var totalStats = launcher.GetTotalStats(); // Aggregates all stats   
```

## 🛠️ Usage Examples

### 🧮 Unit Test
```csharp
public class CalculatorTest : ITestSurface
{
    public string Info => "Tests basic calculator operations";
    public string Tags => "unit, math";
    public string FailureMessage { get; private set; }
    public bool? Passed { get; private set; }
    public bool IsComplete => true;
    public bool IndependentLaunchOnly => false;

    public async Task Start(IDictionary<string, List<string>> args)
    {
        var calc = new Calculator();
        var testCases = new[] { (2, 3, 5), (0, 0, 0), (-1, 1, 0) };

        foreach (var (a, b, expected) in testCases)
        {
            var result = calc.Add(a, b);
            if (result != expected)
            {
                Passed = false;
                FailureMessage = $"Add({a}, {b}): expected {expected}, got {result}";
                return;
            }
        }
        
        Passed = true;
    }
}
```

### 🌐 Integration Test
```csharp
public class ApiTest : ITestSurface
{
    public string Info => "Tests REST API endpoints";
    public string Tags => "integration, api";
    public string FailureMessage { get; private set; }
    public bool? Passed { get; private set; }
    public bool IsComplete { get; private set; }
    public bool IndependentLaunchOnly => false;

    public async Task Start(IDictionary<string, List<string>> args)
    {
        try
        {
            var baseUrl = args.ContainsKey("-baseUrl") ? args["-baseUrl"][0] : "https://localhost:5000";
            var client = new HttpClient { BaseAddress = new Uri(baseUrl) };

            // Create user
            var user = new { Name = "Test", Email = "test@example.com" };
            var response = await client.PostAsJsonAsync("/users", user);
            response.EnsureSuccessStatusCode();
            
            var created = await response.Content.ReadFromJsonAsync<User>();
            
            // Retrieve user
            var getResponse = await client.GetAsync($"/users/{created.Id}");
            var retrieved = await getResponse.Content.ReadFromJsonAsync<User>();
            
            // Compare using deep comparison
            Passed = Assert.SameValues(created, retrieved);
            FailureMessage = Passed == false ? "Created and retrieved users don't match" : null;
            IsComplete = true;
        }
        catch (Exception ex)
        {
            Passed = false;
            FailureMessage = ex.Message;
            IsComplete = false;
        }
    }
}
```

### 💻 Full Program Example
```csharp
using System;
using TestSurface;

namespace Tests
{
    class Program
    {
        static int Main(string[] args)
        {
            var launcher = new SurfaceLauncher();
           
            // (1) Relay the terminal
            // The args should contain +all or +ITestSurfaceTypeName
            launcher.Start(args);

            // (2) Or launch specific tests from code
            // Pass each argument as a separate string to preserve spaces 
            launcher.Start("+TS1", "-option", "with space", "o2", "+TS2");
            launcher.Start("+TS1", "-option", "o3", "o4");
            
            // Check the Results
            var stats = launcher.GetTotalStats();
			
            // Inspect specific instance
            var testInstance = (TS1)launcher.RunHistory[1].Tests[typeof(TS1)].Instance;
			
            // Get the total failures count
            return stats.Failed > 0 ? -stats.Failed : 0;
        }
    }
}
```

## 📝 Command Reference

```bash
# 🚀 Execution
+all                          # Run all tests
+TestName                     # Run specific test
+TestA +TestB                 # Run multiple tests

# 🎯 Filtering  
+all -wtags tag1 tag2         # Tests with any matching tag
+all -wotags tag1 tag2        # Tests without any of these tags
+all -wxtags tag1 tag2        # Tests with all these tags
+all -skip TestA TestB        # Skip specific tests

# 🎛️ Control
+all -info                    # List tests (don't run)
+all +break                   # Stop on first failure
+all +notrace                 # Minimal output
+all +noprint                 # Silent mode

# 📋 Parameters
+MyTest -param value          # Named parameter
+MyTest value1 value2         # Unnamed parameters (stored in "*" key)
```

## 🤖 For AI/LLM Integration

**TestSurface is ideal for AI-generated tests because:**

- **🎯 Single Interface**: Only implement `ITestSurface` - no complex attributes or inheritance
- **📦 Flexible Parameters**: Tests receive `Dictionary<string, List<string>>` from command line or code
- **✅ Clear Results**: Simple `bool? Passed` and `string FailureMessage` properties
- **🔍 Deep Comparison**: `Assert.SameValues()` handles complex object verification automatically
- **💻 Command-Line Native**: Easy to integrate into any workflow or CI/CD pipeline

**AI Prompt for Test Generation:**
```
Generate a TestSurface test class that implements ITestSurface for [scenario].

Requirements:
- Set Info property to describe what the test does
- Set Tags property with relevant comma-separated tags
- In Start() method, set Passed = true/false based on test outcome
- Set FailureMessage when Passed = false
- Use Assert.SameValues() for complex object comparisons
- Handle exceptions by setting Passed = false and FailureMessage = ex.Message
- Use .AsSuccess(), .AsError(), .AsInfo() extension methods for output

Example pattern:
public class [TestName] : ITestSurface
{
    public string Info => "[Description]";
    public string Tags => "[tags]";
    public string FailureMessage { get; private set; }
    public bool? Passed { get; private set; }
    public bool IsComplete { get; private set; }
    public bool IndependentLaunchOnly => false;

    public async Task Start(IDictionary<string, List<string>> args)
    {
        try
        {
            // Test implementation
            Passed = [condition];
            IsComplete = true;
        }
        catch (Exception ex)
        {
            Passed = false;
            FailureMessage = ex.Message;
        }
    }
}
```

## ✨ Why TestSurface?

- **🚀 No Setup Overhead**: One interface, no configuration
- **🔍 Deep Object Comparison**: Handles complex assertions automatically  
- **💻 Command-Line Control**: Run tests selectively with tags and parameters
- **🔗 Framework Agnostic**: Works alongside existing test frameworks
- **🤖 AI-Friendly**: Simple patterns perfect for code generation
- **📦 Minimal Dependencies**: Single NuGet package

Perfect for integration tests, system validation, deployment verification, and any scenario where you need flexible test execution and complex object comparison.