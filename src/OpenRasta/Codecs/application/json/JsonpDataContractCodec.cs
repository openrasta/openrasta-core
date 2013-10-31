using System;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Codecs
{
    [MediaType("application/json; charset=utf-8", "jsonp")]
    public class JsonpDataContractCodec : IMediaTypeReader, IMediaTypeWriter
    {
        private readonly IRequest _request;

        public JsonpDataContractCodec(IRequest request)
        {
            _request = request;
        }

        public object Configuration { get; set; }

        public object ReadFrom(IHttpEntity request, IType destinationType, string paramName)
        {
            if (destinationType.StaticType == null)
                throw new InvalidOperationException();
            return new DataContractJsonSerializer(destinationType.StaticType).ReadObject(request.Stream);

        }

        public void WriteTo(object entity, IHttpEntity response, string[] paramneters)
        {
            var queryParams = HttpUtility.ParseQueryString(_request.Uri.Query);
            if (queryParams["jsoncallback"] != null)
            {
                WriteJsonp(entity, response, queryParams["jsoncallback"]);
            }
            else
            {
                WriteJson(entity, response);
            }
        }

        private static void WriteJsonp(object entity, IHttpEntity response, string jsonpId)
        {
            var front = Encoding.UTF8.GetBytes(jsonpId + "(");
            response.Stream.Write(front, 0, front.Length);
            WriteJson(entity, response);
            var back = Encoding.UTF8.GetBytes(")");
            response.Stream.Write(back, 0, back.Length);
        }

        private static void WriteJson(object entity, IHttpEntity response)
        {
            var serializer = new DataContractJsonSerializer(entity.GetType());
            serializer.WriteObject(response.Stream, entity);
        }
    }
}