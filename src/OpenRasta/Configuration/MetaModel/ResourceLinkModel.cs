using System;

namespace OpenRasta.Configuration.MetaModel
{
  public class ResourceLinkModel : ConfigurationModel
  {
    public string Relationship { get; set; }
    public Uri Uri { get; set; }
    public ResourceLinkCombination CombinationType { get; set; }
    public string Type { get; set; }
  }
}