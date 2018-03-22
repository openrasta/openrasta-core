using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerReturns.Resource
{
  public class get_with_dependency
  {
    InMemoryHost server;

    public get_with_dependency()
    {
      server = new InMemoryHost(() =>
        ResourceSpace.Has
          .ResourcesOfType<string>()
          .AtUri("/string-sync").Named("sync")
          .And.AtUri("/string-async").Named("async")
          .And.AtUri("/string-async-void").Named("async-void")
          .HandledBy<Handler>());
    }

    [Fact]
    public async Task get_with_sync_method()
    {
      var response = await server.Get("/string-sync");
      response.ReadString().ShouldBe("helloworld");
    }

    [Fact]
    public async Task get_with_async_method()
    {
      var response = await server.Get("/string-async");
      response.ReadString().ShouldBe("helloworld");
    }
    
    [Fact]
    public async Task get_with_async_void_method()
    {
      var response = await server.Get("/string-async-void");
      response.StatusCode.ShouldBe(204);
    }

    class Handler
    {
      public Handler(IRequest request)
      {
        
      }

      [HttpOperation(ForUriName = "sync")]
      public OperationResult GetSync()
      {
        return new OperationResult.OK("helloworld");
      }

      [HttpOperation(ForUriName = "async")]
      public Task<OperationResult> GetAsync()
      {
        return Task.FromResult<OperationResult>(new OperationResult.OK("helloworld"));
      }
      
      
      [HttpOperation(ForUriName = "async-void")]
      public Task GetAsyncVoid()
      {
        return Task.CompletedTask;
      }
    }
  }
}