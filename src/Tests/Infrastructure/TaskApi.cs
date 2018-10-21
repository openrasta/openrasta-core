using System.Collections.Generic;
using OpenRasta.Authentication;
using OpenRasta.Authentication.Basic;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Pipeline.Contributors;
using OpenRasta.Web;

namespace Tests.Infrastructure
{
  public class TaskApi : IConfigurationSource
  {
    public void Configure()
    {
      //If request does not pass a Authorization header then pipeline will continue and not try to authenticate with Basic auth
      ResourceSpace.Uses.CustomDependency<IAuthenticationScheme, BasicAuthenticationScheme>(DependencyLifetime.Singleton);
      ResourceSpace.Uses.CustomDependency<IBasicAuthenticator, TestBasicAuthenticator>(DependencyLifetime.Transient);
      ResourceSpace.Uses.PipelineContributor<AuthenticationContributor>();

      ResourceSpace.Has
        .ResourcesOfType<IEnumerable<TaskItem>>()
        .AtUri("/tasks")
        .HandledBy<TaskHandler>()
        .TranscodedBy<NewtonsoftJsonCodec>();

      ResourceSpace.Has
        .ResourcesNamed("health")
        .AtUri("/ping-silently").Named("Silent")
        .And.AtUri("/ping-empty-content").Named("NoContent")
        .HandledBy<TaskApiHealthHandler>();

      ResourceSpace.Has
        .ResourcesOfType<string>()
        .AtUri("/authedtasks")
        .HandledBy<AuthedUserHandler>();
    }
  }

  public class TaskApiHealthHandler
  {
    public void GetSilent()
    {
    }

    public OperationResult.OK GetNoContent() => new OperationResult.OK();
  }
}