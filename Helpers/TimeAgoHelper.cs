namespace BaseProject.Domain.Helpers;

public static class TimeAgoHelper
{
    /// <summary>
    /// Returns a human-friendly description of how long ago the specified date/time was.
    /// </summary>
    /// <param name="dateTime">A past DateTime to compare against now.</param>
    /// <returns>String like "just now", "5 sec ago", "29 min ago", "2 h ago", "3 days ago", etc.</returns>
    public static string GetTimeAgo(DateTime dateTime)
    {
        // Ensure we compare in the same kind (UTC vs local)
        var now = DateTime.UtcNow;
        if (dateTime.Kind == DateTimeKind.Local)
        {
            dateTime = dateTime.ToUniversalTime();
        }
        else if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            // Assume unspecified is UTC; adjust if needed
            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        var span = now - dateTime;

        if (span.TotalSeconds < 5)
            return "just now";
        if (span.TotalSeconds < 60)
            return $"{(int)span.TotalSeconds} sec ago";
        if (span.TotalMinutes < 60)
            return $"{(int)span.TotalMinutes} min ago";
        if (span.TotalHours < 24)
            return $"{(int)span.TotalHours} h ago";
        if (span.TotalDays < 7)
            return $"{(int)span.TotalDays} days ago";
        if (span.TotalDays < 30)
            return $"{(int)(span.TotalDays / 7)} week{Pluralize((int)(span.TotalDays / 7))} ago";
        if (span.TotalDays < 365)
            return $"{(int)(span.TotalDays / 30)} month{Pluralize((int)(span.TotalDays / 30))} ago";

        return $"{(int)(span.TotalDays / 365)} year{Pluralize((int)(span.TotalDays / 365))} ago";
    }

    private static string Pluralize(int quantity)
    {
        return quantity > 1 ? "s" : string.Empty;
    }
}