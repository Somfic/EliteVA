using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using EliteVA.Loggers.File.Formats.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EliteVA.Loggers.File.Formats.Default;

public class DefaultFileFormat : IFileFormat
{
    internal DefaultFileFormat()
    {

    }
    
    private string LogLevelToEmoji(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "🔍",
            LogLevel.Debug => "🐛",
            LogLevel.Information => "📝",
            LogLevel.Warning => "⚠️",
            LogLevel.Error => "❌",
            LogLevel.Critical => "🛑",
            _ => "📝"
        };
    }

    /// <inheritdoc />
    public StringBuilder CreateLogEntry(LogLevel logLevel, string category, EventId eventId, string message, Exception? ex)
    {
        var entry = new StringBuilder();

        var now = DateTime.Now;

        // debug
        entry.Append(LogLevelToEmoji(logLevel));
        
        // 2021/01/23 24:12:23.400
        entry.Append(now.ToString("yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture));

        // Somfic.Logging.Test.Source
        entry.Append($" {category} ");

        entry.Append(message);

        if (ex != null)
        {
            var stack = ex.StackTrace;

            while (ex != null)
            {
                entry.AppendLine();

                // Invalid file format
                entry.Append(GetPrettyExceptionName(ex));

                entry.Append(": ");
                entry.Append(ex.Message.Trim());

                if (ex.Data.Count > 0)
                {
                    entry.AppendLine();
                    entry.Append(JsonConvert.SerializeObject(ex.Data));
                }

                ex = ex.InnerException;
            }

            if (!string.IsNullOrWhiteSpace(stack))
            {
                IEnumerable<string> stackLines = stack.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                stackLines = stackLines.Reverse().ToArray();

                for (var index = 0; index < stackLines.Count(); index++)
                {
                    var stackLine = stackLines.ElementAt(index).Trim();

                    entry.AppendLine();
                    entry.Append(index + 1);
                    entry.Append($": {stackLine}");
                }
            }
        }

        return entry;
    }

    private string GetPrettyExceptionName(Exception ex)
    {
        var output = Regex.Replace(ex.GetType().Name, @"\p{Lu}", m => " " + m.Value.ToLowerInvariant());

        output = ex.GetType().Name == "Exception" ? "Exception" : output.Replace("exception", "");

        output = output.Trim();
        output = char.ToUpperInvariant(output[0]) + output.Substring(1);
        switch (output)
        {
            case "I o":
                output = "IO";
                break;
            case "My sql":
                output = "MySQL";
                break;
            case "Sql":
                output = "SQL";
                break;
        }

        return output;
    }
}