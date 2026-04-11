using Shared.Domain.Abstractions;

namespace Shared.Application.Exceptions;

public sealed class ApplicationException : Exception
{
    public ApplicationException(string requestName, Error? error = null, Exception? innerException = null)
        : base("Application exception", innerException)
    {
        RequestName = requestName;
        Error = error;
    }

    public string RequestName { get; }
    public Error? Error { get; }
}
