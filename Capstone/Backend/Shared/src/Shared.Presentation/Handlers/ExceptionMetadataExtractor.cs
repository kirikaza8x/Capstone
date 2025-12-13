using System.Reflection;

namespace Shared.Presentation.Handler
{
    public static class ExceptionMetadataExtractor
    {
        public static Dictionary<string, object> Extract(Exception exception)
        {
            var metadata = new Dictionary<string, object>();

            // Safely extract all readable properties
            foreach (PropertyInfo prop in exception.GetType().GetProperties())
            {
                if (prop.CanRead)
                {
                    try
                    {
                        var value = prop.GetValue(exception);
                        metadata[prop.Name] = value ?? "null";
                    }
                    catch
                    {
                        metadata[prop.Name] = "Error reading property";
                    }
                }
            }

            // Standard diagnostic fields
            metadata["ExceptionType"] = exception.GetType().Name;
            metadata["Message"] = exception.Message;
            metadata["Source"] = exception.Source ?? "Unknown source";
            metadata["HResult"] = exception.HResult;
            metadata["StackTrace"] = Truncate(exception.StackTrace ?? "No stack trace", 2000);
            metadata["Timestamp"] = DateTime.UtcNow;

            // Capture full inner exception chain
            var inner = exception.InnerException;
            int depth = 0;
            while (inner != null)
            {
                metadata[$"InnerException[{depth}]"] = inner.Message;
                inner = inner.InnerException;
                depth++;
            }

            // Add correlation ID for distributed tracing
            metadata["CorrelationId"] = Guid.NewGuid().ToString();

            return metadata;
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...(truncated)";
        }
    }
}
