using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Web;

namespace OpenRasta.Codecs.application
{
  public class ProblemDetails
  {
    public string Type { get; set; } = "about:blank";
    public string Title { get; set; }
    public string Detail { get; set; }

    public ProblemDetails(Exception exception)
    {
      // Type = exception.HelpLink
    }
  }
}