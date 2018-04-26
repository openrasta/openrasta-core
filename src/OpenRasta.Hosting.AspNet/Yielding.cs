using System.Threading.Tasks;

namespace OpenRasta.Hosting.AspNet
{
  public static class Yielding
  {
    public static async Task<bool> DidItYield(Task pipeline, Task yielded)
    {
      if (pipeline.IsCompleted)
      {
        await pipeline;
        return false;
      }
      if (yielded.IsCompleted)
      {
        await yielded;
        return true;
      }
      var completedTask = await Task.WhenAny(yielded, pipeline);
      return completedTask == yielded;
    }
  }
}