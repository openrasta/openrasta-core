using System;

namespace OpenRasta.Hosting
{
  public class ContextScope : IDisposable
  {
    readonly AmbientContext _previous;

    public ContextScope(AmbientContext context)
    {
      _previous = AmbientContext.Current;
      AmbientContext.Current = context;
    }

    public void Dispose()
    {
      AmbientContext.Current = _previous;
    }
  }
}