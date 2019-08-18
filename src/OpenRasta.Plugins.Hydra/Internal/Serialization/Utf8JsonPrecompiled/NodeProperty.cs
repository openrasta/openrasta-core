using System.Linq.Expressions;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public class NodeProperty
  {
    public string Name { get; }

    public NodeProperty(string name)
    {
      Name = name;
    }

    public InlineCode Code { get; set; }
    public BinaryExpression Conditional { get; set; }
    public InlineCode Preamble { get; set; }
  }
}