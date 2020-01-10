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
    
    public InlineCode ToCode(InlineCode separatorCode)
    {
      var code = separatorCode + Code;

      if (Conditional != null) 
        code = new InlineCode(
          Expression.IfThen(
            Conditional, 
            new CodeBlock(code)));

      return (Preamble + code);
    }
  }
}