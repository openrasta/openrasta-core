using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Pipeline.CallGraph
{
  public class DependentContributorMissingException : Exception
  {
    public IEnumerable<Type> ContributorTypes { get; }

    public DependentContributorMissingException(params Type[] contributorTypes)
      : this((IEnumerable<Type>)contributorTypes)

    {
    }

    DependentContributorMissingException(IEnumerable<Type> contributorTypes)
      : base("Dependent contributor(s) missing, ensure they are added to the pipeline: "
             + string.Join(", ", (contributorTypes = contributorTypes.ToList()).Select(c=>c.Name)))
    {
      ContributorTypes = contributorTypes;
    }
  }
}