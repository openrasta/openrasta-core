using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  public class YieldBeforeNextMiddleware : IPipelineMiddleware, IPipelineMiddlewareFactory
  {
    readonly string _yieldName;

    public YieldBeforeNextMiddleware(string yieldName)
    {
      _yieldName = yieldName;
    }

    public async Task Invoke(ICommunicationContext env)
    {
      var mtid = Thread.CurrentThread.ManagedThreadId;
      var cc = Thread.CurrentContext;
      var stuff = Thread.CurrentThread.ExecutionContext;
      env.Yielder(_yieldName).SetResult(true);
      
      var cc2 = Thread.CurrentContext;
      var stuff2 = Thread.CurrentThread.ExecutionContext;

      var currentContext = HttpContext.Current;
      var shouldContinue = await env.Resumer(_yieldName).Task;
      
      var cc3 = Thread.CurrentContext;
      var stuff3 = Thread.CurrentThread.ExecutionContext;
      
      var clearContext = false; 
      try
      {
        if (HttpContext.Current == null)
        {
          HttpContext.Current = currentContext;
          clearContext = true;
        }
        if (shouldContinue)
          await Next.Invoke(env);
      }
      finally
      {
//        if (clearContext)
//          HttpContext.Current = null;
      }
    }

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      Next = next;
      return this;
    }

    IPipelineMiddleware Next { get; set; }
  }
}