namespace Shared.Application.Abstractions.Messaging;

public interface ITransactionalCommand : ICommand { }

public interface ITransactionalCommand<TResponse> : ICommand<TResponse> { }