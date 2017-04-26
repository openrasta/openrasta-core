using System;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenRasta.Pipeline;
using OpenRasta.Web;
using Shouldly;

namespace OpenRasta.Hosting.AspNet.Tests.Integration
{
  public class AsyncYielding
  {
    [Test]
    public async Task yielding_same_thread()
    {
      var yielded = new TaskCompletionSource<bool>();
      var resumer = new TaskCompletionSource<bool>();

      var resumed = false;
      var pipeline = yieldingCode(
        () => yielded.SetResult(true),
        () => resumer.Task,
        () => code(()=>resumed = true));
      var didIt = await didItYield(pipeline, yielded.Task);

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
          () => code(()=>resumed = true)));
      var didIt = await didItYield(pipeline, yielded.Task);

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
        () => Task.Run(() => code(()=>resumed = true)));
      var didIt = await didItYield(pipeline, yielded.Task);

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
          () => Task.Run(() => code(()=>resumed = true))));
      var didIt = await didItYield(pipeline, yielded.Task);

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
            () => code(()=>resumed = true))));
      var didIt = await didItYield(pipeline, yielded.Task);

      didIt.ShouldBeFalse();

      resumed.ShouldBeFalse();

      resumer.SetResult(true);
      await pipeline;

      resumed.ShouldBeFalse();
    }

    async Task<bool> didItYield(Task pipeline, Task yielded)
    {
      if (pipeline.IsCompleted) return false;
      if (yielded.IsCompleted) return true;
      var completedTask = await Task.WhenAny(yielded, pipeline);
      return completedTask == yielded;
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

}