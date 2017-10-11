using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.DI.Internal
{
  abstract class ResolveProfile
  {
    public abstract bool TryResolve(out object instance);
  }
}