using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.DI.Internal
{
  public class RegistrationBag
  {
    ConcurrentStack<DependencyRegistration> _registrations = new ConcurrentStack<DependencyRegistration>();
    DependencyRegistration _last;

    public void Add(DependencyRegistration registration)
    {
      _registrations.Push(registration);
      Last = registration;
    }

    public bool IsEmpty => _registrations.IsEmpty;
    public IEnumerable<DependencyRegistration> All => _registrations;

    public DependencyRegistration Last
    {
      get => _last;
      private set => _last = value;
    }

    public bool TryRemove(DependencyRegistration registration, out bool isEmpty)
    {
      isEmpty = false;
      var regs = _registrations.ToArray();
      if (!regs.Contains(registration)) return false;

      var newStack = new ConcurrentStack<DependencyRegistration>(
        regs.Where(r => r != registration));
      if (newStack.IsEmpty)
      {
        _last = null;
        isEmpty = true;
      }
      else
      {
        newStack.TryPeek(out _last);
      }
      _registrations = newStack;
      return true;
    }
  }
}