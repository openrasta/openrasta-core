// ReSharper disable UnusedMember.Global -- here for compat

using System;
using System.Web;

namespace OpenRasta.Hosting.AspNet
{
  [Obsolete("Not supported, please remove <httpHandler> and <handler> from your web.config.")]
  public class OpenRastaHandler : IHttpHandler
  {
    public OpenRastaHandler()
    {
      throw new NotSupportedException(
        "Not supported, please remove <httpHandler> and <handler> from your web.config.");
    }

    public bool IsReusable => true;

    public void ProcessRequest(HttpContext context)
    {
      throw new NotSupportedException(
        "Not supported, please remove <httpHandler> and <handler> from your web.config.");
    }
  }
}