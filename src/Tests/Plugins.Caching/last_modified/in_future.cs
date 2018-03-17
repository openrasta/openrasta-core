﻿using System;
using Shouldly;
using Xunit;

namespace Tests.Plugins.Caching.last_modified
{
    public class in_future : contexts.caching
    {
        
        public in_future()
        {
            // 2.2.1
            // An origin server with a clock MUST NOT send a Last-Modified date that
            // is later than the server's time of message origination (Date).
            given_time(now);

            given_resource<TestResource>(map=>map.LastModified(_=> now + TimeSpan.FromMinutes(1)));

            when_executing_request("/TestResource");
        }

        [Fact]
        public void request_successful()
        {
            response.StatusCode.ShouldBe(200);
        }

        [Fact]
        public void last_modified_header_present()
        {
            should_be_date(response.Headers["last-modified"], now);
        }
    }
}