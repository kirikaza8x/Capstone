namespace AI.Domain.ValueObjects
{
    /// <summary>
    /// Single source of truth for all valid action type strings.
    /// Eliminates duplication between UserBehaviorLog helpers and InteractionWeight.
    /// </summary>
    public static class ActionTypes
    {
        // Passive
        public const string View = "view";

        // Engagement
        public const string Click = "click";
        public const string Like = "like";
        public const string Share = "share";
        public const string Comment = "comment";
        public const string Bookmark = "bookmark";

        // Conversion
        public const string Purchase = "purchase";
        public const string Subscribe = "subscribe";
        public const string Checkout = "checkout";
        public const string Signup = "signup";

        public static readonly IReadOnlySet<string> All = new HashSet<string>
        {
            View, Click, Like, Share, Comment, Bookmark,
            Purchase, Subscribe, Checkout, Signup
        };

        public static readonly IReadOnlySet<string> Conversions = new HashSet<string>
        {
            Purchase, Subscribe, Checkout, Signup
        };

        public static readonly IReadOnlySet<string> Engagements = new HashSet<string>
        {
            Click, Like, Share, Comment, Bookmark
        };

        public static bool IsKnown(string actionType) => All.Contains(actionType);
        public static bool IsConversion(string actionType) => Conversions.Contains(actionType);
        public static bool IsEngagement(string actionType) => Engagements.Contains(actionType);
    }
}