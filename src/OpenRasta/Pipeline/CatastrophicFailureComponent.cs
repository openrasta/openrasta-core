using System.Text;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class CatastrophicFailureComponent : IPipelineComponent
  {
    public Task Invoke(ICommunicationContext env)
    {

      try
      {
        string fatalError =
          "An error in one of the rendering components of OpenRasta prevents the error message from being sent back.";
        env.Response.StatusCode = 500;
        env.Response.Entity.ContentLength = fatalError.Length;
        env.Response.Entity.Stream.Write(Encoding.ASCII.GetBytes(fatalError), 0, fatalError.Length);
        env.Response.WriteHeaders();
      }
      catch
      {
      }
      return Task.FromResult(0);
    }
  }
}