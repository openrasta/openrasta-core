using System.Linq;
using Shouldly;
using Tests.OperationModel.Interceptors.Support;
using Xunit;

namespace Tests.OperationModel.Interceptors
{
  public class rewrites : interceptor_scenario
  {
    public rewrites()
    {
      given_operation<GeneralProductsGuaranteeHandler>(handler => handler.AttackWithAntiMatter());
      when_invoking_operation();
    }

    [Fact]
    public void rewritten_value_returned()
    {
      Result.Single().Value.ShouldBe(true);
    }
  }
}