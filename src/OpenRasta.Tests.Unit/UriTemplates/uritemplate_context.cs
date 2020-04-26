using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections.Specialized;
using OpenRasta.Tests.Unit.Infrastructure;

namespace OpenRasta.Tests.Unit.UriTemplates
{
  public abstract class uritemplate_context : context
  {
    protected IEnumerable<Uri> BaseUris = new List<Uri> {new Uri("http://localhost")};

    protected IEnumerable<string> BindingUriByName(string template, object values)
    {
      var boundUris = BaseUris
        .Select(baseUri => new UriTemplate(template)
          .BindByName(baseUri, values.ToNameValueCollection()));
      return boundUris.Select(uri=>uri.ToString()).ToList();
    }

    protected void GivenBaseUris(params string[] uris)
    {
      BaseUris = new List<Uri>(uris.Select(u => new Uri(u)));
    }
  }
}