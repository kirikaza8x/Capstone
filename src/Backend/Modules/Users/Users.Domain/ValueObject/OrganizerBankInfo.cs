using Shared.Domain.DDD;

namespace Users.Domain.ValueObjects;

public sealed class OrganizerBankInfo : ValueObject
{
    public string? AccountName { get; }
    public string? AccountNumber { get; }
    public string? BankCode { get; }
    public string? Branch { get; }

    public OrganizerBankInfo(
        string? accountName,
        string? accountNumber,
        string? bankCode,
        string? branch)
    {
        AccountName = accountName;
        AccountNumber = accountNumber;
        BankCode = bankCode;
        Branch = branch;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return AccountName;
        yield return AccountNumber;
        yield return BankCode;
        yield return Branch;
    }
}