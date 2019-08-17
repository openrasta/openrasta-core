using System.Collections.Generic;
using System.Linq.Expressions;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public class InlineCodeBlock
  {
    public List<Expression> Variables { get; set; } = new List<Expression>();
    public List<Expression> Statements { get; set; } = new List<Expression>();
  }
}