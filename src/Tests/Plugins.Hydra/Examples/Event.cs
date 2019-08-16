using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using OpenRasta.Plugins.Hydra.Schemas;

namespace Tests.Plugins.Hydra.Examples
{
  public class Event : JsonLd.INode
  {
    public Event()
    {
      Customers = new List<Customer>();
    }
    public int Id { get; set; }
    
    public string FirstName { get; set; }

    public Customer Customer { get; set; }

    public List<Customer> Customers { get; set; }

    [JsonIgnore]
    [IgnoreDataMember]
    public int Age { get; set; }
  }
}