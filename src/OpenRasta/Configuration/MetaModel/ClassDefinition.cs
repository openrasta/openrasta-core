using System.Collections.Generic;

namespace OpenRasta.Configuration.MetaModel
{
  public class ClassDefinition
  {
    public ClassDefinition(IEnumerable<ClassProperty> properties, string resourceTypeName, ResourceModel resourceRegistration)
    {
      Properties = properties;
      ResourceTypeName = resourceTypeName;
      ResourceRegistration = resourceRegistration;
    }

    public string ResourceTypeName { get; }

    public ResourceModel ResourceRegistration { get; }

    public IEnumerable<ClassProperty> Properties { get; }
  }
}