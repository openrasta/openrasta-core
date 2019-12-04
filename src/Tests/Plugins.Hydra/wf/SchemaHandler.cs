using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.Plugins.Hydra.wf
{
  // ReSharper disable once ClassNeverInstantiated.Global
  public class SchemaHandler
  {
    public Task<Schema> Get()
    {
      return Task.FromResult(
        new Schema
        {
          new VariableType
          {
            Scheme = "Royal Mail",
            Taxonomy = "Postal Address File",
            Classification = "Postal Address",
            Index = 0,
            Name = "Property Address Key",
            Range = "String",
            AvailableVariables = new List<Variable>
            {
              new Variable
              {
                Scheme = "Royal Mail",
                Taxonomy = "Postal Address File",
                Classification = "Postal Address",
                Provider = "CLS",
                Source = "Titles",
                Provenance = new Provenance
                {
                  Provider = "CLS",
                  Source = "Titles"
                },
                Ghost = false,
                Multiplicity = 2, // A Title can have more than one
                MinPrice = new MonetaryAmount(0.01m, "GBP"),
                MaxPrice = new MonetaryAmount(0.01m, "GBP")
              }
            },
            Section = new KeyValuePair<int, string>(0, "Geography"),
            Group = new KeyValuePair<int, string>(0, "Address"),
          }
        });
    }
  }
}
