using FluentValidation;
using Products.Domain.Products;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Products.Application.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
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

internal class CreateProductCommandHandler(
        IProductRepository productRepository,
        IProductUnitOfWork uow
    ) : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var existingProduct = await productRepository.GetByNameAsync(
           command.Name, cancellationToken);

        if (existingProduct is not null)
        {
            return Result.Failure<Guid>(
                ProductErrors.AlreadyExists(command.Name));
        }

        var product = Product.Create(
            command.Name,
            command.Description,
            command.Price,
            command.Stock);

        productRepository.Add(product);
        await uow.SaveChangesAsync(cancellationToken);
        return Result.Success(product.Id);
    }
}
