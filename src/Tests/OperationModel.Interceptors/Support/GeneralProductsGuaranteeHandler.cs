using System.Threading.Tasks;

namespace Tests.OperationModel.Interceptors
{
  public class GeneralProductsGuaranteeHandler
  {
    [NominalFunction]
    public Task<bool> AttackWithAtomics()
    {
      return Task.FromResult(false);
    }
    [AntiMatter]
    public Task<bool> AttackWithAntiMatter()
    {
      return Task.FromResult(false);
    }

    [UnknownLaserFlaw]
    public Task<bool> AttackWithLaser()
    {
      return Task.FromResult(false);
    }
    /*public Task<bool> HullMalfunction()
    {

    }*/
  }
}