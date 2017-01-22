using System.Web;

namespace OpenRasta.Hosting.AspNet
{
    public static class OpenRastaModuleLoader
    {
        public static void Load()
        {
            HttpApplication.RegisterModule(typeof(OpenRastaModule));
        }
    }
}