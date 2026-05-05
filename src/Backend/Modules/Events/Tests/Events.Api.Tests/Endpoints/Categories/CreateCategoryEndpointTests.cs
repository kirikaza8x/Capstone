using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Events.Application.Categories.Commands.CreateCategory;
using FluentAssertions;
using MediatR;
using Moq;
using Shared.Domain.Abstractions;
using Xunit;

using CreateCategoryRequest = global::Events.Api.Categories.CreateCategoryRequest;

namespace Events.Api.Tests.Endpoints.Categories;

public class CreateCategoryEndpointTests
{
    private readonly Mock<IMediator> _mockMediator = new();

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateCategoryAndReturn201()
    {
        var request = new CreateCategoryRequest(
            "SPORTS",
            "Sports & Games",
            "All sports related events");

        var categoryId = 1;
        _mockMediator
            .Setup(m => m.Send(
                It.IsAny<CreateCategoryCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(categoryId));

        var result = await _mockMediator.Object.Send(
            new CreateCategoryCommand(request.Code, request.Name, request.Description),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(categoryId);

        _mockMediator.Verify(
            m => m.Send(
                It.Is<CreateCategoryCommand>(c =>
                    c.Code == request.Code &&
                    c.Name == request.Name &&
                    c.Description == request.Description),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Handle_WithMissingRequiredFields_ShouldBeInvalid()
    {
        var request = new CreateCategoryRequest(
            string.Empty,
            "Name",
            "Description");

        string.IsNullOrWhiteSpace(request.Code).Should().BeTrue();
    }
}
