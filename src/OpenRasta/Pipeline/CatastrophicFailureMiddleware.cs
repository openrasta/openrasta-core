using System.Text;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class CatastrophicFailureMiddleware : IPipelineMiddleware
  {
    const string FatalError = "An unknown error in one of the rendering components of OpenRasta prevents the error message from being sent back.";

    public Task Invoke(ICommunicationContext env)
    {

      try
      {
        env.Response.StatusCode = 500;
        env.Response.Entity.ContentLength = FatalError.Length;
        env.Response.Entity.Stream.Write(Encoding.ASCII.GetBytes(FatalError), 0, FatalError.Length);
        env.Response.WriteHeaders();
      }
      catch
      {
      }
      return Task.FromResult(0);
    }
  }
}
