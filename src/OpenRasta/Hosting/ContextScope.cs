using System;
using System.Runtime.Remoting.Messaging;

namespace OpenRasta.Hosting
{
  public class ContextScope : IDisposable
  {
    public ContextScope(AmbientContext context)
    {
      if (AmbientContext.Current != null)
        throw new InvalidOperationException("An ambient context already exists");
      AmbientContext.Current = context;
    }

    public void Dispose()
    {
      if (AmbientContext.Current == null)
        throw new InvalidOperationException("An ambient context does not exists");
      AmbientContext.Current = null;
    }
  }
}