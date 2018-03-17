using System;
using System.Collections.Generic;
using OpenRasta.Configuration;

namespace Tests.Plugins.Caching.contexts
{
  public class TestConfiguration : IConfigurationSource
  {
    public List<Action> Has = new List<Action>();
    public List<Action> Uses = new List<Action>();

    public void Configure()
    {
      Uses.ForEach(x => x());
      Has.ForEach(x => x());
    }
  }
}