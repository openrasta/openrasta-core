using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Collections;
using OpenRasta.OperationModel;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public partial class PipelineData
  {
    public PipelineData()
    {
      PipelineStage = new PipelineStage();
      Owin = new OwinData(this);
    }

    T SafeGet<T>(string key) where T : class
    {
      return TryGetValue(key, out var o) ? o as T : null;
    }
    
    public new object this[object key]
    {
      get => TryGetValue(key, out var val) ? val : null;
      set => base[key] = value;
    }
  }
}
