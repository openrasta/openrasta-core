using System.Reflection;

namespace OpenRasta.Configuration.MetaModel
{
  public class ClassDefinition
  {
    public ClassDefinition(PropertyInfo[] propertyInfos, string resourceTypeName)
    {
      this.Properties = propertyInfos;
      this.ResourceTypeName = resourceTypeName;
    }

    public string ResourceTypeName { get; set; }

    public PropertyInfo[] Properties { get; set; }
  }
}