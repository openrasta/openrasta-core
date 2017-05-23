using System.Runtime.Remoting.Messaging;
using System.Web;
using OpenRasta.Pipeline;

namespace OpenRasta.Hosting.AspNet
{
  public class AspNetContextStore : IContextStore
  {
    HttpContext Context => HttpContext.Current ?? ContextFromCallContext;
    HttpContext ContextFromCallContext => CallContext.LogicalGetData("__OR_CONTEXT") as HttpContext;

    public object this[string key]
    {
      get => Context.Items[key];
      set => Context.Items[key] = value;
    }
  }
}