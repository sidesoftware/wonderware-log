# wonderware-log
A simple .net library for writing to the Wonderware/system platform event logger. This library requires that the Wonderware LoggerDLL.dll be present. This component is not intended to be used remotely. Instead, this component is designed to be used in your .net script function library so that you can write to the event log from your application. This component is merely an interop wrapper around the LoggerDLL.dll library.

# Examples
Set the identity name so that you can distinguish your messages in the event logger.
```
WwLog.LogSetIdentityName("SideSoftware");
```

Call the logging functions.

```cs
WwLog.LogInfo(message);
WwLog.LogTrace(message);
WwLog.LogWarning(message
WwLog.LogError(message);
```
That's pretty much it. You can get the nuget package here.

[NuGet package](https://www.nuget.org/packages/wonderware-log)
