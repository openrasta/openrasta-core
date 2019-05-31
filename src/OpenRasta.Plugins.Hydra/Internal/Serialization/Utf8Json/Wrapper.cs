using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using FastMember;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8Json
{
  public class Wrapper : DynamicObject
  {
    [DataMember(Name = "@id")]
    public string Id { get; set; }

    [DataMember(Name = "@context")]
    public string Context { get; set; }

    [DataMember(Name = "@type")]
    public string Type { get; set; }

    private readonly object _value;
    private readonly FastMember.TypeAccessor Accessor;
    private readonly string[] MemberNames;
    private Dictionary<string, string> Map;

    public Wrapper(object value)
    {
      _value = value;
      Accessor = TypeAccessor.Create(value.GetType());

      var atts = value.GetType().GetProperties().SelectMany(p => p.CustomAttributes).SelectMany(g => g.NamedArguments).Where(c => c.MemberName == "Name")
        .Select(v => v.TypedValue.Value.ToString());

      Map = value.GetType().GetProperties()
        .Select(x => new
        {
          Name = x.Name, Att = x.CustomAttributes.SelectMany(v => v.NamedArguments).Where(c => c.MemberName == "Name").Select(v => v.TypedValue.Value.ToString()).FirstOrDefault()
        })
        .ToDictionary(key => key.Name, val => val.Att ?? val.Name);

      if (!Map.ContainsKey("@id"))
      {
        Map.Add("@id", "@id");
      }

      if (!Map.ContainsKey("@context"))
      {
        Map.Add("@context", "@context");
      }

      if (!Map.ContainsKey("@type"))
      {
        Map.Add("@type", "@type");
      }

//            Map = Map.Concat(GetType().GetProperties()
//                .Select(x => new
//                {
//                    Name = x.Name, Att = x.CustomAttributes.SelectMany(v => v.NamedArguments).Where(c => c.MemberName == "Name").Select(v => v.TypedValue.Value.ToString()).FirstOrDefault()
//                })
//                .ToDictionary(key => key.Name, val => val.Att ?? val.Name)).ToDictionary(x => x.Key, x => x.Value);


      var others = value.GetType().GetProperties().Where(x => x.CustomAttributes.All(y => y.AttributeType != typeof(DataMemberAttribute))).Select(c => c.Name);

      var thisatts = GetType().GetProperties().SelectMany(p => p.CustomAttributes).SelectMany(g => g.NamedArguments).Where(c => c.MemberName == "Name")
        .Select(v => v.TypedValue.Value.ToString());

      MemberNames = atts.Concat(others).Concat(thisatts).ToArray();

      MemberNames = Accessor.GetMembers().Select(a => a.Name).Concat(GetType().GetProperties().Select(p => p.Name)).ToArray();

      MemberNames = Map.Values.Select(x => x).ToArray();
    }

    public override IEnumerable<string> GetDynamicMemberNames()
    {
      return MemberNames;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
      switch (binder.Name)
      {
        case "@context":
        {
          result = this.Context;
          return true;
        }

        case "@id":
        {
          result = this.Id;
          return true;
        }

        case "@type":
        {
          result = this.Type;
          return true;
        }

        default:
          if (MemberNames.Contains(binder.Name))
          {
            //result = Accessor[_value, binder.Name];
            result = Accessor[_value, Map.FirstOrDefault(x => x.Value == binder.Name).Key]; //Bit ropey as values might not be unique!!
            return true;
          }

          break;
      }

      return base.TryGetMember(binder, out result);
    }
  }
}