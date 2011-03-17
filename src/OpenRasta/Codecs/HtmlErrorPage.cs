using System.Collections.Generic;
using System.Linq;
using OpenRasta.Resources;
using OpenRasta.Web.Markup;
using OpenRasta.Web.Markup.Elements;

namespace OpenRasta.Codecs
{
    public class HtmlErrorPage : Element
    {
        public HtmlErrorPage(IEnumerable<Error> errors)
        {
            var exceptionBlock = GetExceptionBlock(errors);
            Root = this
                    [html
                             [head
                                      [title["A server error has occured."]]
                                      [style.Type("text/css")[Files.error_css]]
                             ]
                             [body
                                      [h1["A server error has occured."]]
                                      
                                      [div.Class("errorList")[exceptionBlock]]
                             ]
                    ];
        }

        public Element Root { get; set; }

        IDlElement GetExceptionBlock(IEnumerable<Error> errors)
        {
            return errors.Aggregate(dl,
                                    (previous, error) => previous
                                                                 [dt.Class("title")[error.Title]]
                                                                 [dd[pre[error.Message]]]
                                                                 [dt["Exception trace"]]
                                                                 [dd[pre[error.Exception.ToString()]]]
                    );
        }
    }
}