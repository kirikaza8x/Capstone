using MediatR;
using Products.Domain.Products;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Products.Application.Products.Commands.ReduceStockCommand;

internal sealed class ReduceStockCommandHandler : ICommandHandler<ReduceStockCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductUnitOfWork _unitOfWork;

    public ReduceStockCommandHandler(
        IProductRepository productRepository,
        IProductUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReduceStockCommand command, CancellationToken cancellationToken)
    {
        foreach (var item in command.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);

            if (product is null)
                return Result.Failure(ProductErrors.NotFound(item.ProductId));

            if (product.Stock < item.Quantity)
                return Result.Failure(ProductErrors.InsufficientStock(item.ProductId));

            product.UpdateStock(-item.Quantity);
            _productRepository.Update(product);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
