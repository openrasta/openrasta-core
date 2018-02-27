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
      catch (Exception e)
      {
        env.Response.StatusCode = 500;
        env.Response.Entity.ContentLength = FatalError.Length;
        await env.Response.Entity.Stream.WriteAsync(FormatMessage(e), 0, FatalError.Length);
      }
    }

    static byte[] FormatMessage(Exception e)
    {
      return Encoding.ASCII.GetBytes($"{FatalError}{Environment.NewLine}{e}");
    }
  }
}
