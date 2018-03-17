using System;
using OpenRasta.Plugins.Caching;
using Shouldly;
using Xunit;

namespace Tests.Plugins.Caching
{
    public class extensions
    {
        [Fact]
        public void before_date()
        {
            var now = DateTimeOffset.Now;
            now.Before(now + 2.Hours()).ShouldBeTrue();
        }
        [Fact]
        public void after_date()
        {
            var now = DateTimeOffset.Now;
            now.After(now - 2.Hours()).ShouldBeTrue();
        }
    }
}