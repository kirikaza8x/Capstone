using Shared.Domain.Abstractions;

namespace Reports.Domain;

public static class ReportErrors
{
    public static class AdminDashboard
    {
        public static readonly Error DataRetrievalFailed = Error.Failure(
            "AdminDashboard.DataRetrievalFailed",
            "Failed to aggregate data for the admin dashboard. One or more source modules are unavailable.");

        public static Error InvalidPeriod(string period) => Error.Validation(
            "AdminDashboard.InvalidPeriod",
            $"The time period '{period}' is invalid for this report.");
    }

    public static class DateRange
    {
        public static readonly Error InvalidRange = Error.Validation(
            "DateRange.InvalidRange",
            "The start date must be before or equal to the end date.");

        public static readonly Error DateTooFarInPast = Error.Validation(
            "DateRange.DateTooFarInPast",
            "The requested date range exceeds the maximum allowed historical data limit.");
    }

    public static class Integration
    {
        public static Error ModuleDataUnavailable(string moduleName) => Error.Failure(
            "Integration.ModuleDataUnavailable",
            $"Unable to retrieve reporting data from the '{moduleName}' module.");

        public static readonly Error Timeout = Error.Failure(
            "Integration.Timeout",
            "The request to aggregate cross-module data timed out.");
    }
}
