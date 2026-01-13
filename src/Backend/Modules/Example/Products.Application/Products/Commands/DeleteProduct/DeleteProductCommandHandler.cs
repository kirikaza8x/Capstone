using Products.Domain.Products;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Products.Application.Products.Commands.DeleteProduct;

internal sealed class DeleteProductCommandHandler
    : ICommandHandler<DeleteProductCommand>
{
    private readonly IProductRepository _productRepository;

    public DeleteProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(request.Id));
        }

        product.Delete();
        _productRepository.Update(product);

        return Result.Success();
    }
}