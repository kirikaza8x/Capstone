using Products.Domain.Products;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Products.Application.Products.Commands.DeleteProduct;

internal sealed class DeleteProductCommandHandler(
        IProductRepository _productRepository,
        IProductUnitOfWork uow
    )
    : ICommandHandler<DeleteProductCommand>
{
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
        await uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}