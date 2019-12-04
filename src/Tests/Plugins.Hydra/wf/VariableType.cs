using System.Collections.Generic;

namespace Tests.Plugins.Hydra.wf
{
  public class VariableType
  {
    public string Scheme { get; set; }
    public string Taxonomy { get; set; }
    public string Classification { get; set; }
    public int Index { get; set; }
    public string Name { get; set; }
    public string Range { get; set; }
    public IEnumerable<string> PotentialValues { get; set; }
    public IEnumerable<Variable> AvailableVariables { get; set; }
    public VariableTypeGrouping Section { get; set; }
    public VariableTypeGrouping Group { get; set; }
  }
}
