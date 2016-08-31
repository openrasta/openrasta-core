#region License

/* Authors:
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2009 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.DI;
using OpenRasta.Diagnostics;
using OpenRasta.IO;
using OpenRasta.Web;
using OpenRasta.Pipeline;

namespace OpenRasta.Pipeline.Contributors
{
  public class ResponseEntityWriterContributor : KnownStages.IResponseCoding
  {
    static readonly byte[] PADDING = Enumerable.Repeat((byte) ' ', 512).ToArray();

    ILogger Log { get; } = NullLogger.Instance;

    public void Initialize(IPipeline pipeline)
    {
      pipeline.Use(WriteResponseBuffered).After<KnownStages.ICodecResponseSelection>();
    }

    async Task<PipelineContinuation> WriteResponseBuffered(ICommunicationContext context)
    {
      if (context.Response.Entity.Instance == null)
      {
        Log.WriteDebug("There was no response entity, not rendering.");
        await SendEmptyResponse(context);
        return PipelineContinuation.Continue;
      }

      var codecInstance = ResolveCodec(context);
      var writer = CreateWriter(codecInstance);
      using (Log.Operation(this, "Generating response entity."))
      {
        await writer(
          context.Response.Entity.Instance,
          context.Response.Entity,
          context.Request.CodecParameters.ToArray());
        await PadErrorMessageForIE(context);

        if (context.Response.Entity.Stream.CanSeek)
          context.Response.Entity.ContentLength = context.Response.Entity.Stream.Length;
      }

      return PipelineContinuation.Continue;
    }

    ICodec ResolveCodec(ICommunicationContext context)
    {
      var codecInstance = context.Response.Entity.Codec;
      if (codecInstance != null)
      {
        Log.WriteDebug("Codec instance with type {0} has already been defined.",
          codecInstance.GetType().Name);
      }
      else
      {
        context.Response.Entity.Codec =
          codecInstance =
            DependencyManager.GetService(context.PipelineData.ResponseCodec.CodecType) as ICodec;
      }
      if (codecInstance == null)
        throw new CodecNotFoundException(
          $"Codec {context.PipelineData.ResponseCodec.CodecType} couldn't be initialized.");

      Log.WriteDebug("Codec {0} selected.", codecInstance.GetType().Name);

      if (context.PipelineData.ResponseCodec?.Configuration != null)
        codecInstance.Configuration = context.PipelineData.ResponseCodec.Configuration;

      return codecInstance;
    }

    static Func<object, IHttpEntity, IEnumerable<string>, Task> CreateWriter(ICodec codecInstance)
    {
      var codecAsync = codecInstance as IMediaTypeWriterAsync;
      if (codecAsync != null) return codecAsync.WriteTo;
      return (instance, entity, parameters) =>
      {
        ((IMediaTypeWriter) codecInstance).WriteTo(instance, entity, parameters.ToArray());
        return Task.FromResult(0);
      };
    }

    Task SendEmptyResponse(ICommunicationContext context)
    {
      Log.WriteDebug("Writing http headers.");
      context.Response.WriteHeaders();
      return context.Response.Entity.Stream.FlushAsync();
    }

    static Task PadErrorMessageForIE(ICommunicationContext context)
    {
      if ((context.OperationResult.IsClientError || context.OperationResult.IsServerError)
          && context.Response.Entity.Stream.CanSeek
          && context.Response.Entity.ContentType == MediaType.Html
          && context.Response.Entity.Stream.Length <= 512)
      {
        return context.Response.Entity.Stream.WriteAsync(PADDING, 0, (int) (512 - context.Response.Entity.Stream.Length));
      }
      return Task.FromResult(0);
    }
  }

  internal class CodecNotFoundException : Exception
  {
    public CodecNotFoundException(string message) : base(message)
    {
    }
  }
}

#region Full license

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#endregion
