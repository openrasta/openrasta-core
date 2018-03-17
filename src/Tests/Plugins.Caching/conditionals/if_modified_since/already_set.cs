using System;
using OpenRasta.Configuration;
using OpenRasta.Pipeline;
using OpenRasta.Plugins.Caching;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Plugins.Caching.conditionals.if_modified_since
{
    public class last_modified_in_future : contexts.caching
    {
        public last_modified_in_future()
        {
            // 2.2.1
            // An origin server with a clock MUST NOT send a Last-Modified date that
            // is later than the server's time of message origination (Date).
            given_current_time(now);
            given_uses(_ => _.PipelineContributor<LastModifiedInFuture>());
            given_resource<TestResource>();
            given_request_header("if-modified-since", now);
            
            when_executing_request("/TestResource");
        }

        [Fact]
        public void not_modified()
        {
            response.StatusCode.ShouldBe(304);
        }

        [Fact]
        public void last_modified_header_not_overridden()
        {
            DateTimeOffset.Parse(response.Headers["last-modified"])
                .ShouldNotBe(now.Value);
        }
        class LastModifiedInFuture : HeaderSetter
        {
            public LastModifiedInFuture() : base("last-modified", (ServerClock.UtcNow() + TimeSpan.FromSeconds(10)).ToString("R")) { }
        }
    }
    abstract class HeaderSetter : IPipelineContributor
    {
        readonly string _header;
        readonly string _value;

        public HeaderSetter(string header, string value)
        {
            _header = header;
            _value = value;
        }

        public void Initialize(IPipeline pipelineRunner)
        {
            pipelineRunner.Notify(WriteLastModified)
                .Before<KnownStages.IOperationExecution>();
        }

        PipelineContinuation WriteLastModified(ICommunicationContext arg)
        {
            arg.Response.Headers[_header] = _value;
            return PipelineContinuation.Continue;
        }
    }
}