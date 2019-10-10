using System;
using System.Collections.Generic;

namespace OpenRasta.Configuration.MetaModel
{
  public class ClassProperty
  {
    public ClassProperty(string name, Type propertyType, IEnumerable<ResourceModel> resourceModels, string range)
    {
      Name = name;
      PropertyType = propertyType;
      ResourceModels = resourceModels;
      Range = range;
    }

    public Type PropertyType { get; set; }

    public IEnumerable<ResourceModel> ResourceModels { get; set; }

    public string Name { get; }

    public string Range { get; }
  }
}