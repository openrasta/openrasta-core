using System;
using System.Text;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class CatastrophicFailureMiddleware : AbstractMiddleware
  {
    const string FatalError = "An unknown error in one of the rendering components of OpenRasta prevents the error message from being sent back.";

    public override async Task Invoke(ICommunicationContext env)
    {
      try
      {
        await Next.Invoke(env);
      }
      catch (Exception e) when (env.Response.HeadersSent == false)
      {
        env.Response.StatusCode = 500;
        var message = FormatMessage(e);
        env.Response.Entity.ContentLength = message.Length;
        await env.Response.Entity.Stream.WriteAsync(message, 0, message.Length);
      }
    }

    static byte[] FormatMessage(Exception e)
    {
      return Encoding.ASCII.GetBytes($"{FatalError}{Environment.NewLine}{e}");
    }
  }
}
