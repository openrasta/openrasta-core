using System;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;

namespace TestingKeepAlive.Console
{

  class Program
  {
    static void Main(string[] args)
    {
      var address = new Uri("http://localhost:52995/");

      var writer = TextWriter.Synchronized(System.Console.Out);

      var posts = new Thread(() =>
      {
        var iteration = 0;
        while (true)
        {
          try
          {
            var request = CreateRequest(address);
            request.Method = "POST";
            request.ContentType = "application/json";

            using (var str = request.GetRequestStream())
            {
              var bytes = Encoding.UTF8.GetBytes(@"{ ""Data"": [1, 2, 3] }");
              str.Write(bytes, 0, bytes.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
              writer.WriteLine("{0} {1}", ++iteration, response.StatusCode);
            }
          }
          catch (WebException webException)
          {
            var stream = webException.Response?.GetResponseStream();
            if (stream != null)
            {
              using (var reader = new StreamReader(stream))
              {
                writer.WriteLine(reader.ReadToEnd());
              }
            }

            throw;
          }
          catch (Exception exception)
          {
            writer.WriteLine(exception);
            throw;
          }

          Thread.Sleep(10);
        }
      });

      var gets = new Thread(() =>
      {
        var iteration = 0;
        while (true)
        {
          try
          {
            var request = CreateRequest(address);
            request.Method = "GET";

            using (var response = (HttpWebResponse)request.GetResponse())
            {
              writer.WriteLine("{0} {1}", ++iteration, response.StatusCode);
            }
          }
          catch (WebException webException)
          {
            var stream = webException.Response?.GetResponseStream();
            if (stream != null)
            {
              using (var reader = new StreamReader(stream))
              {
                writer.WriteLine(reader.ReadToEnd());
              }
            }

            throw;
          }
          catch (Exception exception)
          {
            writer.WriteLine(exception);
            throw;
          }

          Thread.Sleep(10);
        }

      });

      posts.Start();
      gets.Start();

      posts.Join();
      gets.Join();
    }

    static HttpWebRequest CreateRequest(Uri uri)
    {
      var r = WebRequest.CreateHttp(uri);

      // Uncomment either of the following lines and the issue goes away
      //r.KeepAlive = false;
      //r.ProtocolVersion = HttpVersion.Version10;

      return r;
    }
  }
}