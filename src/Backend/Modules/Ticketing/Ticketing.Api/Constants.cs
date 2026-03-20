namespace Ticketing.Api;

internal static class Constants
{
    internal static class Tags
    {
        public const string Orders = "Ticketing - Orders";
    }

    internal static class Routes
    {
        private const string BaseApi = "api/ticketing";

        public const string Orders = $"{BaseApi}/orders";
        public const string OrderById = $"{Orders}/{{orderId:guid}}";
    }
}
