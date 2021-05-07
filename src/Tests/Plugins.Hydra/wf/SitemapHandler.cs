using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.Plugins.Hydra.wf
{
  // ReSharper disable once ClassNeverInstantiated.Global
  public class SitemapHandler
  {
    public Task<Collection<Collection>> Get()
    {
      return Task.FromResult(
        new Collection<Collection>("Geography", 
          new List<Collection<Variable>>
          {
            new Collection<Variable>("Address", 
              new List<Variable> 
              {
                new Variable
                {
                  Scheme = "Royal Mail",
                  Taxonomy = "Postal Address File",
                  Classification = "Postal Address",
                  Provider = "CLS",
                  Source = "Titles", Name = "Property Address Key"
                }
              })}));
    }
  }
}
