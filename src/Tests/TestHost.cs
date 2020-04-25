using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.Configuration.Fluent;
using OpenRasta.Hosting.InMemory;

namespace Tests
{
  public  class TestHost : InMemoryHost
  {
    public static StartupProperties Defaults = new StartupProperties()
    {
      OpenRasta = {Errors = {HandleAllExceptions = false, HandleCatastrophicExceptions = false}}
    };

    public delegate void TestHostConfiguration(IHas has, IUses uses);
    public TestHost(TestHostConfiguration configuration) : base((() =>
    {
      configuration(ResourceSpace.Has, ResourceSpace.Uses);
    }), startup: Defaults)
    {
    }
    public TestHost(IConfigurationSource configuration) : base(configuration, startup: Defaults)
    {
    }
  }
}