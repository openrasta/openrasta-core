using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace OpenRasta.Hosting
{
  public class AmbientContext
  {
    readonly Hashtable _items = new Hashtable();

    private static readonly AsyncLocal<AmbientContext> _current = new AsyncLocal<AmbientContext>();

    public static AmbientContext Current
    {
      get => _current.Value;
      set => _current.Value = value;
    }

    public object this[string key]
    {
      get => _items[key];
      set => _items[key] = value;
    }
  }
}