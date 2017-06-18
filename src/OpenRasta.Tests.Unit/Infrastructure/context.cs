using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace OpenRasta.Tests.Unit.Infrastructure
{
  [TestFixture]
  public abstract class context
  {
    [SetUp]
    protected virtual void SetUp()
    {
    }

    [TearDown]
    protected virtual void TearDown()
    {
    }

    public Action Executing(Action action)
    {
      return action;
    }

    public Func<Task> Executing(Func<Task> action)
    {
      return action;
    }
  }
}