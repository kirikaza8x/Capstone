using FluentValidation;
using Products.Domain.Products;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Products.Application.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");
    }
}

internal sealed class UpdateProductCommandHandler(
        IProductRepository _productRepository,
        IProductUnitOfWork uow
    )
    : ICommandHandler<UpdateProductCommand>
{

    public async Task<Result> Handle(
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            command.Id,
            cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(command.Id));
        }

        var existingProduct = await _productRepository.GetByNameAsync(
            command.Name,
            cancellationToken);

        if (existingProduct is not null && existingProduct.Id != command.Id)
        {
            return Result.Failure(ProductErrors.AlreadyExists(command.Name));
        }

        product.Update(
            command.Name,
            command.Description,
            command.Price,
            command.Stock);
        _productRepository.Update(product);
        await uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
