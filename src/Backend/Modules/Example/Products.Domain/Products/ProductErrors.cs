using Shared.Domain.Abstractions;

namespace Products.Domain.Products;

public static class ProductErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Products.NotFound",
        $"Product with ID '{id}' was not found");

    public static Error AlreadyExists(string name) => Error.Conflict(
        "Products.AlreadyExists",
        $"Product with name '{name}' already exists");

    public static Error InvalidPrice => Error.Validation(
        "Products.InvalidPrice",
        "Price must be greater than zero");

    public static Error InvalidStock => Error.Validation(
        "Products.InvalidStock",
        "Stock cannot be negative");
}
