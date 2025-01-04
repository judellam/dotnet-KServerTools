namespace KServerTools.Common;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Text;

/// <summary>
/// Custom console formatter for logging.
/// </summary>
/// <remarks>
/// This class is used to create a custom console formatter for logging.
/// Requires: Microsoft.Extensions.Logging.Abstractions
/// Requires: Microsoft.Extensions.Logging.Console
/// </remarks>
public class CustomConsoleFormatter : ConsoleFormatter {
    public const string FormatterName = "CustomFormatter";

    public CustomConsoleFormatter() : base(FormatterName) { }

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter) {
        // Create the log message
        StringBuilder messageBuilder = new();

        // Add the actual log message
        if (logEntry.State is not null) {
            messageBuilder.Append(logEntry.State);
        }

        // Add exception details if any
        if (logEntry.Exception is not null) {
            messageBuilder.AppendLine();
            messageBuilder.AppendLine(logEntry.Exception.ToString());
        }

        // Write the custom log message
        textWriter.WriteLine(messageBuilder.ToString());
    }
}