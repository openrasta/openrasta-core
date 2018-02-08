using System.Collections.Generic;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.Web;

namespace Tests.Infrastructure
{
  public class TaskApi : IConfigurationSource
  {
    public void Configure()
    {
      ResourceSpace.Has
        .ResourcesOfType<IEnumerable<TaskItem>>()
        .AtUri("/tasks")
        .HandledBy<TaskHandler>()
        .TranscodedBy<NewtonsoftJsonCodec>();
      ResourceSpace.Has
        .ResourcesNamed("health")
        .AtUri("/ping-silently").Named("Silent")
        .HandledBy<TaskApiHealthHandler>();
    }
  }

  public class TaskApiHealthHandler
  {
    public void GetSlient(){}
  }
}