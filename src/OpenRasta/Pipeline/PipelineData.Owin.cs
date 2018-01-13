using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace OpenRasta.Pipeline
{
  public partial class PipelineData
  {
    public class OwinData
    {
      readonly PipelineData _pipelineData;

      public OwinData(PipelineData pipelineData)
      {
        _pipelineData = pipelineData;
      }

      public X509Certificate SslClientCertificate
      {
        get => _pipelineData.SafeGet<X509Certificate>(EnvironmentKeys.OWIN_SSL_CLIENT_CERTIFICATE);
        set => _pipelineData[EnvironmentKeys.OWIN_SSL_CLIENT_CERTIFICATE] = value;
      }

      public Func<Task> SslLoadClientCertAsync
      {
        get => _pipelineData.SafeGet<Func<Task>>(EnvironmentKeys.OWIN_SSL_CLIENT_CERTIFICATE_LOAD);
        set => _pipelineData[EnvironmentKeys.OWIN_SSL_CLIENT_CERTIFICATE_LOAD] = value;
      }
    }
  }
}