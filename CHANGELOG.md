# Changelog

## v1.1.1

+ Listing the Debug/Release mode of the discovered assemblies
+ Assembly names in the test header

## v1.1

+ **[braking]** There are two switch levels: [+] for the ITestSurface implementations and [-] for the options
+ **[braking]** the options **all** and **brake** must be prefixed with + 
+ two new launcher options are added: **+noprint** and **-skip**
+ The runner keeps execution history so one could inspect the test instances
  from a specific run, including the test updated input arguments and the unhandled exceptions.
+ The test arguments are sliced as the values between two +T keys. 
+  **[braking]** The *ArgsParser* class is static.
