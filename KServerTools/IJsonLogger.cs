namespace KServerTools.Common;

using System.Runtime.CompilerServices;

/// <summary>
/// Custom logger interface. The regular ILogger interface works well, but this logger will output an event in a specific JSON format.
/// In addition it captures the callers file path, line number, and member name for better debugging.
/// </summary>
public interface IJsonLogger {
    /// <summary>
    /// Logs an informational message when the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition that must be true for the message to be logged.</param>
    /// <param name="message">The log message.</param>
    /// <param name="latency">Optional latency value in milliseconds.</param>
    /// <param name="filePath">The source file path of the caller. Populated automatically.</param>
    /// <param name="lineNumber">The line number of the caller. Populated automatically.</param>
    /// <param name="memberName">The member name of the caller. Populated automatically.</param>
    void IfInfo(
        bool condition,
        string message,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "");

    /// <summary>
    /// Logs an error message when the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition that must be true for the error to be logged.</param>
    /// <param name="message">The log message.</param>
    /// <param name="exception">The exception associated with the error.</param>
    /// <param name="latency">Optional latency value in milliseconds.</param>
    /// <param name="filePath">The source file path of the caller. Populated automatically.</param>
    /// <param name="lineNumber">The line number of the caller. Populated automatically.</param>
    /// <param name="memberName">The member name of the caller. Populated automatically.</param>
    void IfError(
        bool condition,
        string message,
        Exception exception,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "");

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="latency">Optional latency value in milliseconds.</param>
    /// <param name="filePath">The source file path of the caller. Populated automatically.</param>
    /// <param name="lineNumber">The line number of the caller. Populated automatically.</param>
    /// <param name="memberName">The member name of the caller. Populated automatically.</param>
    void Info(
        string message,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "");

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="exception">Optional exception associated with the warning.</param>
    /// <param name="latency">Optional latency value in milliseconds.</param>
    /// <param name="filePath">The source file path of the caller. Populated automatically.</param>
    /// <param name="lineNumber">The line number of the caller. Populated automatically.</param>
    /// <param name="memberName">The member name of the caller. Populated automatically.</param>
    void Warn(
        string message,
        Exception? exception = null,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "");

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="exception">The exception associated with the error.</param>
    /// <param name="latency">Optional latency value in milliseconds.</param>
    /// <param name="filePath">The source file path of the caller. Populated automatically.</param>
    /// <param name="lineNumber">The line number of the caller. Populated automatically.</param>
    /// <param name="memberName">The member name of the caller. Populated automatically.</param>
    void Error(
        string message,
        Exception exception,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "");
}
