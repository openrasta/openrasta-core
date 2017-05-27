namespace OpenRasta.Tests.Unit.DI
{
  public class Dependent<T>
  {
    T _dep;

    public Dependent(T dependency)
    {
      _dep =dependency;
    }
    public T GetConstructorDependency()
    {
      return _dep;
    }
  }
}