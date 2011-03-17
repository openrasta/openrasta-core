JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
#region License
/* Authors:
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2009 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */
#endregion

using System.Collections.Generic;
using System.IO;
using OpenRasta.Web;
using OpenRasta.Web.Markup;
using OpenRasta.Web.Markup.Rendering;

namespace OpenRasta.Codecs
{
    /// <summary>
    /// Codec rendering error messages collected during the processing of a request.
    /// </summary>
    [MediaType("application/xhtml+xml;q=0.9")]
    [MediaType("text/html")]
    [SupportedType(typeof(IList<Error>))]
    public class HtmlErrorCodec : IMediaTypeWriter
    {
        public object Configuration { get; set; }

        public void WriteTo(object entity, IHttpEntity response, string[] paramneters)
        {
            var errors = entity as IList<Error>;
            if (errors == null)
                return;
            response.ContentType = MediaType.Html;
            using (var streamWriter = new StreamWriter(response.Stream))
            {
                var writer = new XhtmlNodeWriter();
                writer.Write(new XhtmlTextWriter(streamWriter), new HtmlErrorPage(errors));
            }
        }
    }
}

#region Full license
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion