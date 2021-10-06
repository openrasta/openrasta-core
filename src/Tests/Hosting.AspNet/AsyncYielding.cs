#if NET472_OR_GREATER
using System;
using System.Threading.Tasks;
using OpenRasta.Hosting.AspNet;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Hosting.AspNet
{
  public class AsyncYielding
  {
    [Fact]
    public async Task middleware_yielding_same_thread()
    {
      var didItYield = await InvokeUntil<CodeMiddleware>(
        new YieldBefore<CodeMiddleware>(),
        new CodeMiddleware(() => Resumed = true));

      didItYield.ShouldBeTrue();
      Resumed.ShouldBeFalse();

      await Resume<CodeMiddleware>();

      Resumed.ShouldBeTrue();
    }

    [Fact]
    public async Task middleware_yielding_other_thread()
    {
      var didItYield = await InvokeUntil<CodeMiddleware>(
        new OtherThreadMiddleware(),
        new YieldBefore<CodeMiddleware>(),
        new CodeMiddleware(() => Resumed = true)
      );


      didItYield.ShouldBeTrue();
      Resumed.ShouldBeFalse();

      await Resume<CodeMiddleware>();

      Resumed.ShouldBeTrue();
    }

    [Fact]
    public async Task middleware_yielding_before_code_on_other_thread()
    {
      var didItYield = await InvokeUntil<OtherThreadMiddleware>(
        new YieldBefore<OtherThreadMiddleware>(),
        new OtherThreadMiddleware(),
        new CodeMiddleware(() => Resumed = true)
      );


      didItYield.ShouldBeTrue();
      Resumed.ShouldBeFalse();

      await Resume<OtherThreadMiddleware>();

      Resumed.ShouldBeTrue();
    }

    [Fact]
    public async Task middleware_not_yielding_same_thread()
    {
      var didItYield = await InvokeUntil<CodeMiddleware>(
        new BypassingCodeMiddleware(),
        new YieldBefore<CodeMiddleware>(),
        new CodeMiddleware(() => Resumed = true)
      );

      didItYield.ShouldBeFalse();
      Resumed.ShouldBeFalse();

      await Resume<CodeMiddleware>();

      Resumed.ShouldBeFalse();
    }

    [Fact]
    public async Task not_yielding_different_thread()
    {
      var didItYield = await InvokeUntil<CodeMiddleware>(
        new OtherThreadMiddleware(),
        new BypassingCodeMiddleware(),
        new YieldBefore<CodeMiddleware>(),
        new CodeMiddleware(() => Resumed = true)
      );


      didItYield.ShouldBeFalse();
      Resumed.ShouldBeFalse();

      await Resume<CodeMiddleware>();

      Resumed.ShouldBeFalse();
    }


    readonly ICommunicationContext Env;

    public AsyncYielding()
    {
      Env = new InMemoryCommunicationContext();
      Resumed = false;
    }

    bool Resumed { get; set; }

    async Task Resume<T>()
    {
      Env.Resumer(typeof(T).Name).SetResult(true);
      await Operation;
    }

    async Task<bool> InvokeUntil<T>(params IPipelineMiddlewareFactory[] factories)
    {
      Operation = factories.Compose().Invoke(Env);

      return await Yielding.DidItYield(
        Operation,
        Env.Yielder(typeof(T).Name).Task);
    }

    Task Operation { get; set; }
  }

  class BypassingCodeMiddleware : IPipelineMiddleware, IPipelineMiddlewareFactory
  {
    public Task Invoke(ICommunicationContext env)
    {
      return Task.FromResult(true);
    }

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      return this;
    }
  }

  class OtherThreadMiddleware : IPipelineMiddleware, IPipelineMiddlewareFactory
  {
    public Task Invoke(ICommunicationContext env)
    {
      return Task.Run(() => Next.Invoke(env));
    }

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      Next = next;
      return this;
    }

    IPipelineMiddleware Next { get; set; }
  }

  class CodeMiddleware : IPipelineMiddleware, IPipelineMiddlewareFactory
  {
    readonly Action _action;

    public CodeMiddleware(Action action)
    {
      _action = action;
    }

    public Task Invoke(ICommunicationContext env)
    {
      _action();
      return Task.FromResult(true);
    }

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      return this;
    }
  }
}
#endif
