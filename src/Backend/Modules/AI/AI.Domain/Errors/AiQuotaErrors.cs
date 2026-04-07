using Shared.Domain.Abstractions;

namespace AI.Domain.Errors;

public static class AiQuotaErrors
{
    public static Error TokenAmountMustBePositive => Error.Validation(
        "AiQuota.TokenAmountMustBePositive",
        "Token amount must be greater than zero.");

    public static Error InsufficientTokens(int requested, int available) => Error.Validation(
        "AiQuota.InsufficientTokens",
        $"Insufficient AI tokens. Requested: {requested}, Available: {available}.");

    public static Error InvalidSubscriptionExpiry => Error.Validation(
        "AiQuota.InvalidSubscriptionExpiry",
        "Subscription expiry must be in the future.");
}
