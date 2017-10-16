using OpenRasta.DI.Internal;

namespace OpenRasta.Pipeline
{
  public interface IContextStoreDependencyCleaner
  {
    void Destruct(string key, object instance);
  }
}