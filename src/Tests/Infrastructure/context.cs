using System;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Infrastructure
{
  public abstract class context
  {
    protected static Action Executing(Action action)
    {
      return action;
    }

    protected static Func<Task> Executing(Func<Task> action)
    {
      return action;
    }

  }
}