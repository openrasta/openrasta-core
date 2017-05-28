namespace OpenRasta.Tests.Unit.DI
{
  public class Dependent<T> : IDependent<T>
  {
    T _dep;

    public Dependent(T dependency)
    {
      _dep =dependency;
    }
    public T CtorDependencies()
    {
      return _dep;
    }
  }

  public interface IDependent<T>
  {
    T CtorDependencies();
  }
}