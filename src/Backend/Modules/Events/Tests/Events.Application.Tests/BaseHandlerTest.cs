using System.Threading;
using MediatR;
using Moq;

namespace Events.Application.Tests
{
    /// <summary>
    /// Base class for handler tests using MediatR Mediator
    /// Since handlers are internal sealed, we test through mediator interface
    /// </summary>
    public abstract class BaseHandlerTest
    {
        protected readonly Mock<IMediator> MockMediator;

        protected BaseHandlerTest()
        {
            MockMediator = new Mock<IMediator>();
        }

        /// <summary>
        /// Setup mediator to send command/query and return result
        /// </summary>
        protected void SetupMediatorSend<TRequest, TResponse>(
            TRequest request,
            TResponse response)
            where TRequest : IRequest<TResponse>
        {
            MockMediator
                .Setup(m => m.Send(
                    It.Is<TRequest>(r => r.Equals(request)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
        }

        /// <summary>
        /// Setup mediator to send any command/query and return result
        /// </summary>
        protected void SetupMediatorSendAny<TRequest, TResponse>(
            TResponse response)
            where TRequest : IRequest<TResponse>
        {
            MockMediator
                .Setup(m => m.Send(
                    It.IsAny<TRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
        }
    }
}
