namespace OpenRasta.DI.Internal
{
  abstract class ResolveProfile
  {
    public abstract bool TryResolve(out object instance);
  }
}