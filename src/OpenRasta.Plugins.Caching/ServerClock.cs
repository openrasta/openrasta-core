using System;

namespace OpenRasta.Plugins.Caching
{
  public static class ServerClock
  {
    public static Func<DateTimeOffset> UtcNowDefinition = () => DateTimeOffset.UtcNow;

    public static DateTimeOffset UtcNow()
    {
      return UtcNowDefinition();
    }
  }
}