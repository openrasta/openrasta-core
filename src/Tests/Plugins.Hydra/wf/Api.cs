using System;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.Plugins.Diagnostics;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.ReverseProxy;
using OpenRasta.Web;

namespace Tests.Plugins.Hydra.wf
{

  public static class Ontologies
  {
    public const string WhenFreshApi = "https://api.whenfresh.com/ontologies/wf/api/#";
  }

  public class Api : IConfigurationSource
  {
    public void Configure()
    {
      ResourceSpace.Uses.Hydra(opts => { opts.Vocabulary = Ontologies.WhenFreshApi; });
      ResourceSpace.Has
        .ResourcesOfType<Schema>()
        .Vocabulary(Ontologies.WhenFreshApi)
        .AtUri("/schema")
        .HandledBy<SchemaHandler>();

      ResourceSpace.Has
        .ResourcesOfType<Ontology>()
        .Vocabulary(Ontologies.WhenFreshApi)
        .AtUri(o => $"/ontologies/{o.Scheme}/{o.Taxonomy}");

      ResourceSpace.Has
        .ResourcesOfType<VariableType>()
        .Vocabulary(Ontologies.WhenFreshApi)
        .AtUri(s => $"/ontologies/{s.Scheme}/{s.Taxonomy}#{s.Classification}/{s.Name}");

      ResourceSpace.Has
        .ResourcesOfType<Provenance>()
        .Vocabulary(Ontologies.WhenFreshApi)
        .AtUri(p => $"/vars/{p.Provider}/{p.Source}");

      ResourceSpace.Has
        .ResourcesOfType<Variable>()
        .Vocabulary(Ontologies.WhenFreshApi)
        .AtUri(v => $"/vars/{v.Provider}/{v.Source}#{v.Scheme}/{v.Taxonomy}/{v.Classification}/{v.Name}");
    }
  }
}