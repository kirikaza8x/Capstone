using Shared.Application.Messaging;

namespace Users.Application.Messaging
{
    /// <summary>
    /// Marker interface for all user-related commands.
    /// </summary>
    public interface IUserSaveCommand 
    {
    }

    
    /// <summary>
    /// Marker interface for transactional user commands.
    /// </summary>
    public interface ITransactionalUserCommand : IUserSaveCommand, ITransactionalCommand
    {
    }
}
