namespace Shared.Application.Abstractions.Messaging
{
    /// <summary>
    /// Marker interface to indicate that a command should automatically save changes
    /// after execution via the UnitOfWorkBehavior pipeline.
    /// </summary>
    public interface ITransactionalCommand
    {
    }
}