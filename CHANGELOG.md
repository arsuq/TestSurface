# Changelog

## v1.2

+ **Tags**. With ```+all -wtags tag1 tag2``` one can easily start groups of tests.
+ The *ITestSurface* has a Tags property.
+ The **Runner** is renamed to **SurfaceLauncher**.
+ The **Run** methods in both the *Launcher* and the *ITestSurface* interface are renamed to **Start**.
+ A command mode for the launcher with the **+cmd** -commandName switch
+ The launcher has a *PrintTestInfo* flag which controls the printing of the Info text after the test header. 

> @TheUpgraders, apologies for the renaming inconvenience.


## v1.1.4

+ Removed all background colors in Print.

## v1.1.3

+ the *+break* option treats !Passed.HasValue as !Passed.Value and will stop launching tests.

## v1.1.2

+ The trace tries to lock with a Monitor now.
+ The *LockAwaitMS* is infinite (-1) by default.
+ *ThrowOnLockTimeout* is false, i.e. setting the *LockAwaitMS* to 10 for example
  will silently drop traces. The default setup will lag, but log everything.

## v1.1.1

+ Listing the Debug/Release mode of the discovered assemblies
+ Assembly names in the test header

## v1.1

+ There are two switch levels: [+] for the ITestSurface implementations and [-] for the options
+ the options **all** and **break** must be prefixed with + 
+ two new launcher options are added: **+noprint** and **-skip**
+ The runner keeps execution history so one could inspect the test instances
  from a specific run, including the test updated input arguments and the unhandled exceptions.
+ The test arguments are sliced as the values between two +T keys. 
+ The *ArgsParser* class is static.
