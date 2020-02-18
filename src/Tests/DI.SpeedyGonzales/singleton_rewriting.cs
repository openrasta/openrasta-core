using Xunit;

namespace Tests.DI.SpeedyGonzales
{
  public class singleton_rewriting
  {
    DependencyGraphBuilder graph;

    public singleton_rewriting()
    {
      graph = new DependencyGraphBuilder(new SpeedyGonzalesResolver()
      {
        context => context.Transient((Hero hero, Artifact artifact) => new Quest(hero, artifact)),
        context => context.Singleton(() => new Hero("Heracles")),
        context => context.Transient(() => new Artifact("Hydra"))
      }.Registrations);
    }

    [Fact]
    public void has_three_original_nodes()
    {
      graph.Nodes[0].ShouldBe<Quest>("(hero, artifact) => new Quest(hero, artifact)");
      graph.Nodes[1].ShouldBe<Hero>("() => new Hero(\"Heracles\")");
      graph.Nodes[2].ShouldBe<Artifact>("() => new Artifact(\"Hydra\")");
    }

    [Fact]
    public void transients_are_rewritten()
    {
      graph.RewrittenNodes[0].ShouldBe<Quest>("hero => new Quest(hero, new Artifact(\"Hydra\"))");
      graph.RewrittenNodes[1].ShouldBe<Hero>("() => new Hero(\"Heracles\")");
      graph.RewrittenNodes[2].ShouldBe<Artifact>("() => new Artifact(\"Hydra\")");
    }
  }
}