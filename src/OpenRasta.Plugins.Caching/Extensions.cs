using System;

namespace OpenRasta.Plugins.Caching
{
  public static class Extensions
  {
    public static TimeSpan Minutes(this int minutes)
    {
      return TimeSpan.FromMinutes(minutes);
    }

    public static TimeSpan Hours(this int hours)
    {
      return TimeSpan.FromHours(hours);
    }

    public static bool Before(this DateTimeOffset? origin, DateTimeOffset? date)
    {
      return origin < date;
    }

    public static bool After(this DateTimeOffset? origin, DateTimeOffset? date)
    {
      return origin > date;
    }

    public static bool Before(this DateTimeOffset origin, DateTimeOffset? date)
    {
      return origin < date;
    }

    public static bool After(this DateTimeOffset origin, DateTimeOffset? date)
    {
      return origin > date;
    }
  }
}