using System.Collections.Generic;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;

namespace Tests.Infrastructure
{
  public class TaskApi : IConfigurationSource
  {
    public void Configure()
    {
      using (OpenRastaConfiguration.Manual)
      {
        ResourceSpace.Has
          .ResourcesOfType<IEnumerable<TaskItem>>()
          .AtUri("/tasks")
          .HandledBy<TaskHandler>()
          .TranscodedBy<NewtonsoftJsonCodec>();
      }
    }
  }
}