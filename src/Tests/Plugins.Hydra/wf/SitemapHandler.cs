using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.Plugins.Hydra.wf
{
  // ReSharper disable once ClassNeverInstantiated.Global
  public class SitemapHandler
  {
    public SiteMap Get()
    {
      return new SiteMap("Geography",
        new Collection<Variable>("Address",
          new Variable
          {
            Scheme = "Royal Mail",
            Taxonomy = "Postal Address File",
            Classification = "Postal Address",
            Provider = "CLS",
            Source = "Titles", Name = "Property Address Key"
          }
        ));
    }
  }
}