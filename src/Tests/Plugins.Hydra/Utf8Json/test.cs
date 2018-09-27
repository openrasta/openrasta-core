using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.Hydra.Internal;
using OpenRasta.Reflection;
using Shouldly;
using Xunit;

namespace Tests.Plugins.Hydra.Utf8Json
{
  public class test
  {
    [Fact]
    public async Task dostuff()
    {
      var customer = new Customer() {Name = "woop"};
      var repository = new MetaModelRepository(() => new IMetaModelHandler[] {new PreCompiledUtf8JsonSerializer()});
      var resourceModel = new ResourceModel()
      {
        ResourceKey = typeof(Customer)
      };
      repository.ResourceRegistrations.Add(resourceModel);
      repository.Process();

      var opts = new SerializationOptions {BaseUri = new Uri("http://localhost/")};
      var ms = new MemoryStream();

      await resourceModel.Hydra().SerializeFunc(customer, opts, ms);
      ms.Position = 0;
      var result = Encoding.UTF8.GetString(ms.ToArray());
      result.ShouldBe("not the value");
    }
  }
}