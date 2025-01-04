namespace KServerTools.Common;

using System.Runtime.CompilerServices;

/// <summary>
/// Custom logger interface. The regular ILogger interface works well, but this logger will outpout an event in a specific JSON format.
/// In addition it captures the callers file path, line number, and member name for better debugging.
/// </summary>
public interface IJsonLogger {
    void Info(
        string message,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "");

    void Warn(
        string message,
        Exception? exception = null,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""); 

    void Error(
        string message,
        Exception exception,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "");
}