using System;

namespace OpenRasta.Web.Configuration.Wadl
{
  public class WadlHandler
  {
    readonly Func<ICommunicationContext> _env;
    readonly IUriResolver _uriRepository;

    public WadlHandler(Func<ICommunicationContext> env, 
      IUriResolver uriRepository)
    {
      _env = env;
      _uriRepository = uriRepository;
    }

    public WadlApplication Get()
    {
      var templateProcessor = _uriRepository as IUriTemplateParser;
      if (templateProcessor == null)
        throw new InvalidOperationException(
          "The system doesn't have a IUriTemplateParser, WADL generation cannot proceed.");

      var app = new WadlApplication
      {
        Resources =
        {
          BasePath = _env().ApplicationBaseUri.ToString()
        }
      };

      foreach (var uriMap in _uriRepository)
      {
        var resource = new WadlResource {Path = uriMap.UriTemplate};

        var templateParameters = templateProcessor.GetTemplateParameterNamesFor(uriMap.UriTemplate);
        var queryParameters = templateProcessor.GetQueryParameterNamesFor(uriMap.UriTemplate);


        resource.Parameters = new System.Collections.ObjectModel.Collection<WadlResourceParameter>();
        foreach (var parameter in templateParameters)
          resource.Parameters.Add(
            new WadlResourceParameter {Style = WadlResourceParameterStyle.Template, Name = parameter});

        foreach (var parameter in queryParameters)
          resource.Parameters.Add(
            new WadlResourceParameter {Style = WadlResourceParameterStyle.Query, Name = parameter});

        // TODO: For each parameter, lookup the parameter type from the matched handler and include the xsd type in it

        app.Resources.Add(resource);
      }
      return app;
    }
  }
}