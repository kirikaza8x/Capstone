using System.Reflection;

namespace Shared.Domain.Common.Exceptions.Handler
{
    public static class ExceptionMetadataExtractor
    {
        public static Dictionary<string, object> Extract(Exception exception)
        {
            var metadata = new Dictionary<string, object>();

            foreach (PropertyInfo prop in exception.GetType().GetProperties())
            {
                if (prop.CanRead)
                {
                    var value = prop.GetValue(exception);
                    metadata[prop.Name] = value ?? "null";
                }
            }

            metadata["ExceptionType"] = exception.GetType().Name;
            metadata["Message"] = exception.Message;
            metadata["StackTrace"] = exception.StackTrace ?? "No stack trace";
            metadata["Timestamp"] = DateTime.UtcNow;

            if (exception.InnerException != null)
            {
                metadata["InnerException"] = exception.InnerException.Message;
            }

            return metadata;
        }
    }
}
