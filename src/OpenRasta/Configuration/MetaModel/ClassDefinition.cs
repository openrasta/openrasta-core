using System;
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

  public class ClassProperty
  {
    public ClassProperty(string name, Type propertyType)
    {
      Name = name;
      PropertyType = propertyType;
    }

    public ClassProperty(string name, Type propertyType, ResourceModel resourceModel)
    {
      Name = name;
      PropertyType = propertyType;
      ResourceModel = resourceModel;
    }

    public Type PropertyType { get; set; }

    public ResourceModel ResourceModel { get; set; }

    public string Name { get; }
  }
}