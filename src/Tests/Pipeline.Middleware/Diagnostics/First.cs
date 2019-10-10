using OpenRasta.Pipeline.Contributors;
using Tests.Pipeline.Initializer.Examples;

namespace Tests.Pipeline.Middleware.Diagnostics
{
  class First : AfterContributor<PreExecutingContributor>
  {
  }
}