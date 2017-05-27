namespace OpenRasta.Tests.Unit.DI
{
  public class TypeWithTwoConstructors
  {
    public ISimple _argOne;
    public IAnother _argTwo;

    public TypeWithTwoConstructors()
    {
    }

    public TypeWithTwoConstructors(ISimple argOne, IAnother argTwo)
    {
      _argOne = argOne;
      _argTwo = argTwo;
    }

    public TypeWithTwoConstructors(ISimple argOne, IAnother argTwo, string somethingElse)
    {
    }
  }
}