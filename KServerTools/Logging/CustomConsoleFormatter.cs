namespace KServerTools.Common;

using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

/// <summary>
/// Custom console formatter for logging.
/// </summary>
/// <remarks>
/// This class is used to create a custom console formatter for logging.
/// Requires: Microsoft.Extensions.Logging.Abstractions
/// Requires: Microsoft.Extensions.Logging.Console.
/// </remarks>
public class CustomConsoleFormatter : ConsoleFormatter {
    /// <summary>
    /// The name of this custom formatter used for registration.
    /// </summary>
    public const string FormatterName = "CustomFormatter";

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomConsoleFormatter"/> class.
    /// </summary>
    public CustomConsoleFormatter() : base(FormatterName) { }

    /// <summary>
    /// Writes the log entry to the specified <see cref="TextWriter"/> using a simplified format.
    /// </summary>
    /// <typeparam name="TState">The type of the log entry state.</typeparam>
    /// <param name="logEntry">The log entry to format.</param>
    /// <param name="scopeProvider">An optional scope provider.</param>
    /// <param name="textWriter">The text writer to write the formatted log to.</param>
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
