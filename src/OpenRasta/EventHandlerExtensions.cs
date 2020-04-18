using System;

namespace OpenRasta
{
  public static class EventHandlerExtensions
  {
    [Obsolete]
    public static void Raise(this EventHandler handler, object src)
    {
      handler?.Invoke(src, EventArgs.Empty);
    }

    [Obsolete]
    public static void Raise(this EventHandler handler, object src, EventArgs args)
    {
      handler?.Invoke(src, args);
    }

    [Obsolete]
    public static void Raise<T>(this EventHandler<T> handler, object src, T args)
      where T : EventArgs
    {
      handler?.Invoke(src, args);
    }
  }
}