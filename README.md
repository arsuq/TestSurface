![](TestSurface.png)

# Test Surface

[![.NET](https://github.com/arsuq/TestSurface/actions/workflows/dotnet.yml/badge.svg)](https://github.com/arsuq/TestSurface/actions/workflows/dotnet.yml)

> **Simple testing framework with one interface, deep object comparison, and command-line control.**

## 🎯 Why TestSurface Exists

If you're reading this, chances are you've experienced one (or all) of these frustrations:

- **⚙️ Setup Paralysis**: Spending hours configuring test frameworks instead of writing tests
- **🏷️ Attribute Hell**: Decorating every method with `[Test]`, `[Fact]`, `[TestMethod]`, `[Setup]`, `[TearDown]`...
- **🎭 Magic Dependencies**: Your tests break because some invisible framework magic changed
- **🔧 Integration Nightmares**: Different test types require different runners, different configurations

**TestSurface was born from a simple belief: Testing should amplify your productivity, not drain it.**

## 📋 The Core Interface

TestSurface revolves around one simple interface - implement it and you're ready to test!

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

### 🎯 Implementation Example

Clean, compact implementation that fits on one screen:

```csharp
public class OrderTest : ITestSurface
{
    public string Info => "Tests order processing validation";
    public string Tags => "order, validation";
    public string FailureMessage { get; private set; }
    public bool? Passed { get; private set; }
    public bool IsComplete { get; private set; }
    public bool IndependentLaunchOnly => false;

    public async Task Start(Dictionary<string, List<string>> args)
    {
        try
        {
            // Process test data
            var result = await ProcessOrder();
            
            // Expected order structure - deep comparison demo
            var expected = new
            {
                orderId = 1001,
                customer = new { id = 501, name = "John Doe" },
                items = new[] 
                {
                    new { id = 1, name = "Product A", price = 29.99, quantity = 2 },
                    new { id = 2, name = "Product B", price = 15.50, quantity = 1 }
                },
                total = 75.48,
                status = "completed"
            };

            // One line compares entire structure - no manual checks needed!
            Passed = Assert.SameValues(expected, result);
            IsComplete = true;
        }
        catch (Exception ex)
        {
            Passed = false;
            FailureMessage = $"Test failed: {ex.Message}";
        }
    }
    
    private async Task<object> ProcessOrder()
    {
        // your code
    }
}
```

## 🚀 Quick Start

### Two Launch Modes - Total Flexibility

1. **🎯 Targeted Testing**: Run specific tests by name
   ```bash
   +MyAwesomeTest -iterations 100 +AnotherTest -timeout 5000
   ```

2. **🌐 Comprehensive Testing**: Discover and run ALL compatible tests
   ```bash
   +all -wtags performance,integration
   ```

> 📝 **Pro Tip**: All test implementations must have a default constructor - keeping things simple!

### 📋 Command Arguments Made Simple

TestSurface's argument parsing is intuitive and powerful. Here's how it works:

| Argument Pattern | Result | Example |
|------------------|--------|---------|
| `+switch` | Test selector | `+MyTest` launches MyTest |
| `-option value` | Option with value | `-timeout 5000` sets timeout |
| `value` (no prefix) | Default arguments | `defaultValue` goes to `*` key |
| `+all` | Run all tests | Discovers all ITestSurface types |

**Example in action:**
```csharp
// This command:
launcher.Start("+TS", "nolead", "-leadOption", "value");

// Creates this argument map for the test:
// leadOption → ["value"]
// * → ["nolead"]
```

### 🎛️ Launcher Options - Power When You Need It

TestSurface gives you fine-grained control over test execution:

| Option | Description | Example |
|--------|-------------|---------|
| `-info` | Show test info instead of running | `+all -info` shows all test descriptions |
| `+/-notrace` | Enable/disable Print.Trace() output | Global trace control |
| `+noprint` | Disable ALL printing | Equivalent to `Print.IgnoreAll = true` |
| `+break` | Stop on first failure | Fail-fast behavior |
| `-skip T1 T2` | Skip specific tests with +all | `+all -skip SlowTest FlakyTest` |
| `-wtags tag1 tag2` | Run tests with ANY matching tag | Tag-based test selection |
| `-wotags tag1 tag2` | Run tests with NO matching tags | Exclusion by tags |
| `-wxtags tag1 tag2` | Run tests with ALL matching tags | Strict tag matching |
| `+cmd -tagstats` | Show tag statistics | Great for test organization |

**💡 Remember**: Tag options must be used with `+all` and only one tag option can be used at a time!

### 🔍 Deep Object Comparison Made Easy

**Stop writing tedious comparison code!** TestSurface's `Assert.SameValues()` does the heavy lifting for you with recursive, reflective object comparison.

**Why you'll love it:**
- 🎯 **Deep comparison**: Recursively compares entire object graphs
- ⚡ **Performance optimized**: Type reflection cached for speed
- 🔧 **Flexible**: Control comparison depth with BindingFlags
- 📦 **Collection-aware**: Handles arrays, lists, and enumerables seamlessly

```csharp
public Task Start(IDictionary<string, List<string>> args)
{
    // Set up your expected result template
    var expected = new
    {
        success = true,
        metrics = new double[] { 1.012, 0.001, 3.912 },
        details = new { message = "Operation completed", code = 200 }
    };

    // Your test produces actual results
    var actual = RunYourTestOperation();

    // One line to compare everything - no manual property-by-property checks!
    Passed = Assert.SameValues(expected, actual);

    return Task.CompletedTask;
}
```

> 🚀 **Pro Tip**: Clear the type cache with `Assert.ClearTypeCache()` if you need fresh reflection data!

### 📝 Smart Output Management

**Take control of your test output** without cluttering your code with conditional statements.

**Key benefits:**
- 🎛️ **Global control**: Suppress output with `-notrace` or `-noprint` flags
- 🔒 **Thread-safe**: Built-in synchronization for multi-threaded tests
- ⏱️ **Timeout-aware**: Configurable lock timeouts to prevent deadlocks
- 🎯 **Selective**: Disable specific output types while keeping others

**⚠️ Performance Note**: For high-performance scenarios, set `Print.SerializeTraces = false` and manage synchronization externally.

### 📊 Complete Test History

**Never lose test context again!** TestSurface maintains detailed records of every test run.

**Access comprehensive test data:**
```csharp
var launcher = new SurfaceLauncher();

// Run tests
launcher.Start(args1);
launcher.Start(args2);

// Dive into results
var runRecord = launcher.RunHistory[0];          // Complete run statistics
var testRecord = runRecord.Tests[typeof(MyTest)]; // Individual test details
var testInstance = (MyTest)testRecord.Instance;   // The actual test instance

// Analyze everything:
// - Original arguments (testRecord.ArgsMap)
// - Exceptions (testRecord.Exception) 
// - Test state and results

var overallStats = launcher.GetTotalStats();     // Aggregate all run data
```

## 🚀 Launching Tests

### From Command Line

The most common way to launch tests - perfect for CI/CD and terminal usage:

```csharp
static int Main(string[] args)
{
    var launcher = new SurfaceLauncher();
    
    // Simply relay terminal arguments - TestSurface does the rest!
    launcher.Start(args);
    
    // Return meaningful exit code based on results
    var stats = launcher.GetTotalStats();
    return stats.Failed > 0 ? -stats.Failed : 0;
}
```

**Command line examples:**
```bash
# Run specific tests
MyTestApp.exe +PerformanceTest -iterations 1000 +IntegrationTest -timeout 30000

# Run all tests with specific tags
MyTestApp.exe +all -wtags smoke,fast

# Show test information without running
MyTestApp.exe +all -info
```

### Programmatically

Launch tests directly from code with full control:

```csharp
var launcher = new SurfaceLauncher();

// Launch specific tests with precise arguments
launcher.Start("+PerformanceTest", "-iterations", "1000", "-timeout", "30000");
launcher.Start("+IntegrationTest", "-database", "testdb", "-users", "100");

// Run the same test with different parameters
launcher.Start("+LoadTest", "-concurrent", "10", "-duration", "60");
launcher.Start("+LoadTest", "-concurrent", "50", "-duration", "120");

// Get comprehensive results
var overallStats = launcher.GetTotalStats();
Console.WriteLine($"Total tests: {overallStats.Total}, Failed: {overallStats.Failed}");

// Inspect individual test instances
var testRecord = launcher.RunHistory[0].Tests[typeof(PerformanceTest)];
var testInstance = (PerformanceTest)testRecord.Instance;
Console.WriteLine($"Final result: {testInstance.Passed}");
}
```

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


## 🎉 Get Started Today!

TestSurface is designed to be simple, flexible, and powerful. Whether you're testing:
- 🧪 Unit tests
- 🔗 Integration tests
- 🧭 End-to-end tests
- 🎯 Performance tests

TestSurface gives you the tools without the complexity. Start writing tests that matter, not configuring frameworks that don't.

---

**Happy Testing! 🚀**

