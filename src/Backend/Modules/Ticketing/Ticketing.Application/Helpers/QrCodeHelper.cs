namespace Ticketing.Application.Helpers;

internal static class QrCodeHelper
{
    private const string Prefix = "AIPROMO";
    private static readonly string Header = $"{Prefix}-";

    public static string Generate(Guid orderTicketId) =>
        $"{Header}{orderTicketId:N}";

    public static bool TryParse(string qrCode, out Guid orderTicketId)
    {
        orderTicketId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(qrCode) ||
            !qrCode.StartsWith(Header, StringComparison.OrdinalIgnoreCase))
            return false;

        return Guid.TryParse(qrCode[Header.Length..], out orderTicketId);
    }
}
