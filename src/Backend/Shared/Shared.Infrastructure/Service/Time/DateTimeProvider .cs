using Shared.Application.Abstractions.Time;

namespace Shared.Infrastructure.Service.Time;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}