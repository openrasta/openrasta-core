namespace OpenRasta.Tests.Unit.DI
{
  public class Another : IAnother
  {
    public Another(ISimple simple)
    {
      Dependent = simple;
    }

    public ISimple Dependent { get; set; }
  }
}