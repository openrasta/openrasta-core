using System.Collections;
using System.Collections.Generic;
using OpenRasta.Pipeline;

namespace OpenRasta.Hosting.Katana
{
  public class OwinContextStore : IContextStore
  {
    readonly IDictionary store;

    public OwinContextStore()
    {
      store = new Dictionary<string, object>();
    }

    public object this[string key]
    {
      get => store[key];
      set => store[key] = value;
    }
  }
}