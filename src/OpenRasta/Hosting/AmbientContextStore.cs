using OpenRasta.Pipeline;

namespace OpenRasta.Hosting
{
  public class AmbientContextStore : IContextStore
  {
    public object this[string key]
    {
      get => AmbientContext.Current[key];
      set => AmbientContext.Current[key] = value;
    }
  }
}