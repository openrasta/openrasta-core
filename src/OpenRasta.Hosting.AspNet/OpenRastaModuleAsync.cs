using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Web;
using OpenRasta.Concordia;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  public class OpenRastaModuleAsync : IHttpModule
  {
    public void Dispose()
    {
    }

    static AspNetPipeline Pipeline => _pipeline.Value;

    static readonly Lazy<AspNetPipeline> _pipeline = new Lazy<AspNetPipeline>(() =>
      HttpRuntime.UsingIntegratedPipeline
        ? (AspNetPipeline) new IntegratedPipeline()
        : new ClassicPipeline(), LazyThreadSafetyMode.PublicationOnly);

    private static readonly Lazy<(AspNetHost host, PipelineStageAsync<KnownStages.IUriMatching> resolveStep)> _host 
      = new Lazy<(AspNetHost host, PipelineStageAsync<KnownStages.IUriMatching>)>(() =>
    {
      var host = new AspNetHost(DefaultStartupProperties());
      HostManager.RegisterHost(host);
      var resolveStep = new PipelineStageAsync<KnownStages.IUriMatching>(host, Pipeline);
      return (host, resolveStep);
    });

    public void Init(HttpApplication application)
    {
      application.AddOnPostResolveRequestCacheAsync(DelayedStepBegin(), DelayedStepEnd());
      application.EndRequest += HandleHttpApplicationEndRequestEvent;
    }

    private EndEventHandler DelayedStepEnd()
    {
      return _host.Value.resolveStep.End;
    }

    private BeginEventHandler DelayedStepBegin()
    {
      return _host.Value.resolveStep.Begin;
    }

    private static StartupProperties DefaultStartupProperties()
    {
      return new StartupProperties
      {
        OpenRasta =
        {
          Pipeline =
          {
            ContributorTrailers =
            {
              [call => call.Target is KnownStages.IUriMatching] =
              () => new YieldBefore<KnownStages.IUriMatching>()
            }
          }
        }
      };
    }

    const string COMM_CONTEXT_KEY = "__OR_COMM_CONTEXT";

    void HandleHttpApplicationEndRequestEvent(object sender, EventArgs e)
    {
      var httpContext = ((HttpApplication) sender).Context;
      CallContext.LogicalSetData("__OR_CONTEXT", httpContext);
      var commContext = (ICommunicationContext) httpContext.Items[COMM_CONTEXT_KEY];
      if (commContext.PipelineData.ContainsKey("openrasta.hosting.aspnet.handled"))
        _host.Value.host.RaiseIncomingRequestProcessed(commContext);
    }
  }
}