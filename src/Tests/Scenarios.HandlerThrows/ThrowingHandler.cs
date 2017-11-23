using System;

namespace Tests.Scenarios.HandlerThrows
{
  class ThrowingHandler
  {
    public void Get()
    {
      throw new Exception("This is an exception");
    }
  }
}