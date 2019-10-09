using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public class PostExecuteMiddleware : AbstractContributorMiddleware
  {
    public PostExecuteMiddleware(ContributorCall call) : base(call)
    {
    }

    public override async Task Invoke(ICommunicationContext env)
    {
      try
      {
        await ContributorInvoke(env);
      }
      catch (Exception e)
      {
        env.ServerErrors.Add(new Error
        {
          Title = "Aborted pipeline",
          Message = "A middleware or contributor in post execute threw an exception.",
          Exception = e
        });
      }
    }
  }
}