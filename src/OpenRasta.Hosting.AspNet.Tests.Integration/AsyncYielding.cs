using System;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline;
using OpenRasta.Web;
using Shouldly;

namespace OpenRasta.Hosting.AspNet.Tests.Integration
{
  public class AsyncYielding
  {
    ICommunicationContext Env;

    [SetUp]
    public void set_environment()
    {
      Env = new InMemoryCommunicationContext();
      Resumed = false;
    }

    public bool Resumed { get; set; }

    [Test]
    public async Task middleware_yielding_same_thread()
    {
      var pipeline = new IPipelineMiddlewareFactory[]
      {
        new YieldingMiddleware(nameof(YieldingMiddleware)),
        new CodeMiddleware(() => Resumed = true)
      }.Compose();

      var operation = pipeline.Invoke(Env);

      var didIt = await Yielding.DidItYield(operation, Env.Yielder(nameof(YieldingMiddleware)).Task);

      didIt.ShouldBeTrue();
      Resumed.ShouldBeFalse();

      Env.Resumer(nameof(YieldingMiddleware)).SetResult(true);
      await operation;

      Resumed.ShouldBeTrue(); 
    }

    [Test]
    public async Task yielding_same_thread()
    {
      var yielded = new TaskCompletionSource<bool>();
      var resumer = new TaskCompletionSource<bool>();

      var resumed = false;
      var pipeline = yieldingCode(
        () => yielded.SetResult(true),
        () => resumer.Task,
        () => code(() => resumed = true));
      var didIt = await Yielding.DidItYield(pipeline, yielded.Task);

      didIt.ShouldBeTrue();
      resumed.ShouldBeFalse();

      resumer.SetResult(true);
      await pipeline;

      resumed.ShouldBeTrue();
    }

    [Test]
    public async Task yielding_other_thread()
    {
      var yielded = new TaskCompletionSource<bool>();
      var resumer = new TaskCompletionSource<bool>();

      bool resumed = false;
      var pipeline = Task.Run(
        () => yieldingCode(
          () => yielded.SetResult(true),
          () => resumer.Task,
          () => code(() => resumed = true)));
      var didIt = await Yielding.DidItYield(pipeline, yielded.Task);

      didIt.ShouldBeTrue();
      resumed.ShouldBeFalse();

      resumer.SetResult(true);
      await pipeline;

      resumed.ShouldBeTrue();
    }

    [Test]
    public async Task yielding_before_code_on_other_thread()
    {
      var yielded = new TaskCompletionSource<bool>();
      var resumer = new TaskCompletionSource<bool>();

      bool resumed = false;

      var pipeline = yieldingCode(
        () => yielded.SetResult(true),
        () => resumer.Task,
        () => Task.Run(() => code(() => resumed = true)));
      var didIt = await Yielding.DidItYield(pipeline, yielded.Task);

      didIt.ShouldBeTrue();
      resumed.ShouldBeFalse();

      resumer.SetResult(true);
      await pipeline;
      resumed.ShouldBeTrue();
    }

    [Test]
    public async Task not_yielding_same_thread()
    {
      var yielded = new TaskCompletionSource<bool>();
      var resumer = new TaskCompletionSource<bool>();

      bool resumed = false;

      var pipeline = bypassingCode(
        () => yieldingCode(
          () => yielded.SetResult(true),
          () => resumer.Task,
          () => Task.Run(() => code(() => resumed = true))));
      var didIt = await Yielding.DidItYield(pipeline, yielded.Task);

      didIt.ShouldBeFalse();

      resumed.ShouldBeFalse();

      resumer.SetResult(true);
      await pipeline;

      resumed.ShouldBeFalse();
    }

    [Test]
    public async Task not_yielding_different_thread()
    {
      var yielded = new TaskCompletionSource<bool>();
      var resumer = new TaskCompletionSource<bool>();

      bool resumed = false;

      var pipeline = Task.Run(
        () => bypassingCode(
          () => yieldingCode(
            () => yielded.SetResult(true),
            () => resumer.Task,
            () => code(() => resumed = true))));
      var didIt = await Yielding.DidItYield(pipeline, yielded.Task);

      didIt.ShouldBeFalse();

      resumed.ShouldBeFalse();

      resumer.SetResult(true);
      await pipeline;

      resumed.ShouldBeFalse();
    }

    Task bypassingCode(Func<Task> next)
    {
      return Task.FromResult(true);
    }

    async Task yieldingCode(Action yield, Func<Task> resumer, Func<Task> next)
    {
      yield();
      await resumer();
      await next();
    }

    Task code(Action onCode = null)
    {
      onCode?.Invoke();
      return Task.FromResult(true);
    }
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