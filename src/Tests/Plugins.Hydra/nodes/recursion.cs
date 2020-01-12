using System;
using Newtonsoft.Json.Linq;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled;
using Shouldly;
using Xunit;

namespace Tests.Plugins.Hydra.nodes
{
  public class recursion
  {
    class A
    {
      public B B { get; set; }
    }

    class B
    {
      public A A { get; set; }
    }

    [Fact]
    void cannot_recurse()
    {
      Should.Throw<InvalidOperationException>(() => new InMemoryHost(() =>
        {
          ResourceSpace.Uses.Hydra(options =>
          {
            options.Vocabulary = "https://schemas.example/schema#";
            options.Serializer = ctx => ctx.Transient(() => new PreCompiledUtf8JsonHandler()).As<IMetaModelHandler>();
          });

          ResourceSpace.Has.ResourcesOfType<A>()
            .Vocabulary("https://schemas.example/schema#");
          ResourceSpace.Has.ResourcesOfType<B>()
            .Vocabulary("https://schemas.example/schema#");
        },
        startup: new StartupProperties
          {OpenRasta = {Errors = {HandleAllExceptions = false, HandleCatastrophicExceptions = false}}}));
    }
  }
}