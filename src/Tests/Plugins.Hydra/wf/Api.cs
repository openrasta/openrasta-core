using System;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;
using OpenRasta.Plugins.Diagnostics;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.ReverseProxy;
using OpenRasta.Web;

namespace Tests.Plugins.Hydra.wf
{
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
        .ResourcesOfType<SiteMap>()
        .Vocabulary(Ontologies.WhenFreshApi)
        .AtUri("/sitemap/test")
        .HandledBy<SitemapHandler>();

      ResourceSpace.Has
        .ResourcesOfType<Collection<Variable>>()
        .Vocabulary(Ontologies.WhenFreshApi);
      
      ResourceSpace.Has
        .ResourcesOfType<Ontology>()
        .Vocabulary(Ontologies.WhenFreshApi)
        .AtUri(o => $"/ontologies/{o.Scheme}/{o.Taxonomy}");

      ResourceSpace.Has
        .ResourcesOfType<VariableType>()
        .Vocabulary(Ontologies.WhenFreshApi)
        .AtUri(s => $"/ontologies/{s.Scheme}/{s.Taxonomy}#{s.Classification}/{s.Name}");

      ResourceSpace.Has
        .ResourcesOfType<VariableTypeGrouping>()
        .Vocabulary(Ontologies.WhenFreshApi);

      ResourceSpace.Has
        .ResourcesOfType<Provenance>()
        .Vocabulary(Ontologies.WhenFreshApi)
        .AtUri(p => $"/vars/{p.Provider}/{p.Source}");

      ResourceSpace.Has
        .ResourcesOfType<Variable>()
        .Vocabulary(Ontologies.WhenFreshApi)
        .AtUri(v => $"/vars/{v.Provider}/{v.Source}#{v.Scheme}/{v.Taxonomy}/{v.Classification}/{v.Name}");



      ResourceSpace.Has
        .ResourcesOfType<LandRegDatum>()
        .Vocabulary(Ontologies.WhenFreshApi)
        .AtUri("/datum")
        .HandledBy<LandRegDatumHandler>();
      
      
      ResourceSpace.Has
        .ResourcesOfType<LandRegAddressCatalog>()
        .Vocabulary(Ontologies.WhenFreshApi)
        .AtUri(x => $"/internal/gb-eaw/addresses/{x.ResourceIdentifier}/catalogs/title")
        .HandledBy<LandRegAddressCatalogHandler>();
    }
  }

  public class LandRegDatumHandler
  {
    public LandRegDatum Get()
    {
      return new LandRegDatum("kjsdfhkjfsd0", "kfdsjkhfsd", "ksjdhfkjhs", "name")
      {
        Variable = new Variable()
        {
          Scheme = "scheme",
          Taxonomy = "blah",
          Classification = "blah",
          Name = "foff",
          Provider = "prov",
          Source = "source"
        },
        Price = new MonetaryAmount(0.1m, "GBP"),
        Value = "12"
      };
    }
  }
}