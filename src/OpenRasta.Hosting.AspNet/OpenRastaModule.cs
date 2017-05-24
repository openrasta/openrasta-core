using System;
using System.Web;


namespace OpenRasta.Hosting.AspNet
{
  // ReSharper disable once ClassNeverInstantiated.Global
  [Obsolete("Not supported anymore, please remove <module> and <httpModule> from your web.config.")]
  public class OpenRastaModule : IHttpModule
  {
    public void Dispose()
    {
    }

    public void Init(HttpApplication app)
    {
      throw new NotSupportedException(
        "Not supported anymore, please remove <module> and <httpModule> from your web.config.");
    }
  }
}