using System.Linq;
using Shouldly;
using Tests.OperationModel.Interceptors.Support;
using Xunit;

namespace Tests.OperationModel.Interceptors.async_attributes
{
  public class no_rewriting : interceptor_scenario
  {
    public no_rewriting()
    {
      given_operation<GeneralProductsGuaranteeHandler>(handler => handler.AttackWithAtomics());
      when_invoking_operation();
    }

    [Fact]
    public void original_value_returned()
    {
      Result.Single().Value.ShouldBe(false);
    }
  }
}