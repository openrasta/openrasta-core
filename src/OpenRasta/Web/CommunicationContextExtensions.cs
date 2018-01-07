using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace OpenRasta.Web
{
  public static class CommunicationContextExtensions
  {
    public static async Task<X509Certificate2> GetClientCertificateAsync(this ICommunicationContext ctx)
    {
      var owinCert = ctx.PipelineData.Owin.SslClientCertificate;
      if (owinCert != null) return CertToCert2(owinCert);


      var certLoader = ctx.PipelineData.Owin.SslLoadClientCertAsync;
      if (certLoader == null) return null;
      await certLoader();

      return CertToCert2(ctx.PipelineData.Owin.SslClientCertificate);
    }

    static X509Certificate2 CertToCert2(X509Certificate cert)
    {
      return cert is X509Certificate2 cert2 ? cert2 : new X509Certificate2(cert);
    }
  }
}