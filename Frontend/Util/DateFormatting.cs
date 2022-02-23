namespace Frontend.Util;

public class DateFormatting
{
	public static string FormatAsElapsedTime(TimeSpan ts)
	{
		if (ts.Days > 0)
			return $"{ts.Days} days";
		if (ts.Hours > 0)
			return $"{ts.Hours} hours";
		if (ts.Minutes > 0)
			return $"{ts.Minutes} minutes";
		if (ts.Seconds > 0)
			return $"{ts.Seconds} seconds";
		if (ts.Milliseconds > 0)
			return $"{ts.Milliseconds} ms";

		throw new ArgumentOutOfRangeException();
	}

	public static string FormatAsTimeAgo(TimeSpan ts)
	{
		if (ts.Days > 0)
			return $"{ts.Days} days ago";
		if (ts.Hours > 0)
			return $"{ts.Hours} hours ago";
		if (ts.Minutes > 0)
			return $"{ts.Minutes} minutes ago";
		if (ts.Seconds > 0)
			return $"{ts.Seconds} seconds ago";
		if (ts.Milliseconds > 0)
			return $"{ts.Milliseconds} ms ago";

		return "Now";
	}

	public static string FormatAsTimeAgo(DateTime dt)
	{
		if (dt.Kind is not DateTimeKind.Utc)
			throw new ArgumentOutOfRangeException(nameof(dt), dt, $"Value must be in UTC");

		var now = DateTime.UtcNow;

		if (dt > now)
			throw new ArgumentOutOfRangeException(nameof(dt), dt, "Value must be in the past");

		return FormatAsTimeAgo(now - dt);
	}
}