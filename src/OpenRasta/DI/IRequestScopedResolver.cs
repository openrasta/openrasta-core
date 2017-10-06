using System;

namespace OpenRasta.DI
{
  public interface IRequestScopedResolver
  {
    IDisposable CreateRequestScope();
  }
}