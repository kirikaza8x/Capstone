namespace Shared.Application.Messaging;

public interface ITransactionalCommand : ICommand { }

public interface ITransactionalCommand<TResponse> : ICommand<TResponse> { }