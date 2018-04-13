using System.Web;
using OpenRasta.Hosting.AspNet;

[assembly: PreApplicationStartMethod(typeof(OpenRastaModuleLoader), nameof(OpenRastaModuleLoader.Load))]
