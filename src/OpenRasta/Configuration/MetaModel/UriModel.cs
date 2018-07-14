using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace OpenRasta.Configuration.MetaModel
{
  public class UriModel : ConfigurationModel
  {
    public ResourceModel ResourceModel { get; set; }
    public CultureInfo Language { get; set; }
    public string Name { get; set; }
    public string Uri { get; set; }
    public ICollection<OperationModel> Operations { get; } = new List<OperationModel>();
  }
}