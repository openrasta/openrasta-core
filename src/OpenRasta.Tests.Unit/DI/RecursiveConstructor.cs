namespace OpenRasta.Tests.Unit.DI
{
  public class RecursiveConstructor
  {
    public RecursiveConstructor(RecursiveConstructor constructor)
    {
    }
  }
}