namespace Reports.Api;

internal static class Constants
{
    internal static class Tags
    {
        public const string Admin = "Admin - Reports";
    }

    internal static class Routes
    {
        private const string BaseApi = "api";
        public const string Admin = $"{BaseApi}/admin/reports";
        public const string AdminOverview = $"{Admin}/overview";
       public const string AdminSalesTrend = $"{Admin}/sales-trend";

    }
}
