using System;

namespace OpenRasta.DI
{
  public class ActionOnDispose : IDisposable
  {
    private readonly Action _onDispose;

    public ActionOnDispose(Action onDispose)
    {
      _onDispose = onDispose;
    }
    public void Dispose()
    {
      _onDispose();
    }
  }
}