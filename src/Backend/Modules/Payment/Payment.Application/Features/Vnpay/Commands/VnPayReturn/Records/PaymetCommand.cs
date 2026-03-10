using Payment.Application.Features.VnPay.Dtos;
using Shared.Application.Abstractions.Messaging;

public record VnPayReturnCommand(
        Guid ItemId,
        string TransactionNo,
        string ResponseCode
    ) : ICommand<VnPayResultDto>;


public class VnPayReturnQueriesCommand : ICommand<VnPayResultDto>
{
    //[FromQuery(Name = "vnp_Amount")]
    public string? VnpAmount { get; set; }

    //[FromQuery(Name = "vnp_BankCode")]
    public string? VnpBankCode { get; set; }

    //[FromQuery(Name = "vnp_BankTranNo")]
    public string? VnpBankTranNo { get; set; }

    //[FromQuery(Name = "vnp_CardType")]
    public string? VnpCardType { get; set; }

    //[FromQuery(Name = "vnp_OrderInfo")]
    public string? VnpOrderInfo { get; set; }

    //[FromQuery(Name = "vnp_PayDate")]
    public string? VnpPayDate { get; set; }

    //[FromQuery(Name = "vnp_ResponseCode")]
    public string? VnpResponseCode { get; set; }

    //[FromQuery(Name = "vnp_TmnCode")]
    public string? VnpTmnCode { get; set; }

    //[FromQuery(Name = "vnp_TransactionNo")]
    public string? VnpTransactionNo { get; set; }

    //[FromQuery(Name = "vnp_TransactionStatus")]
    public string? VnpTransactionStatus { get; set; }

    //[FromQuery(Name = "vnp_TxnRef")]
    public string? VnpTxnRef { get; set; }

    //[FromQuery(Name = "vnp_SecureHash")]
    public string? VnpSecureHash { get; set; }

    public long Amount => long.TryParse(VnpAmount, out var a) ? a : 0;
}
