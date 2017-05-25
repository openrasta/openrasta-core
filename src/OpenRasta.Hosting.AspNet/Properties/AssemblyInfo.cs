using System.Reflection;
using System.Web;
using OpenRasta.Hosting.AspNet;

[assembly: PreApplicationStartMethod(typeof(OpenRastaModuleLoader), nameof(OpenRastaModuleLoader.Load))]
[assembly: AssemblyTitle("OpenRasta asp.net Support")]
[assembly: AssemblyDescription("Support for IIS / asp.net hosting of OpenRasta.")]