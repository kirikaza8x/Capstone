namespace Ticketing.Application.Helpers;

internal static class QrCodeHelper
{
    private const string Prefix = "AIPROMO";
    private static readonly string Header = $"{Prefix}-";

    public static string Generate(Guid orderTicketId, Guid eventSessionId) =>
        $"{Header}{orderTicketId:N}-{eventSessionId:N}";

    public static bool TryParse(string qrCode, out Guid orderTicketId, out Guid eventSessionId)
    {
        orderTicketId = Guid.Empty;
        eventSessionId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(qrCode) ||
            !qrCode.StartsWith(Header, StringComparison.OrdinalIgnoreCase))
            return false;

        // Cut prefix AIPROMO-
        var dataPart = qrCode[Header.Length..];

        // split the remaining part by '-' 
        var parts = dataPart.Split('-');

        if (parts.Length != 2)
            return false;

        // Parse guid
        if (Guid.TryParse(parts[0], out orderTicketId) &&
            Guid.TryParse(parts[1], out eventSessionId))
        {
            return true;
        }

        return false;
    }
}
