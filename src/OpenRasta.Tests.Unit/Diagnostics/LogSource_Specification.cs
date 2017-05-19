using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenRasta.Diagnostics;
using OpenRasta.Testing;
using Shouldly;

namespace OpenRasta.Diagnostics
{
    public class when_reading_log_source_data
    {
        [Test]
        public void the_category_is_read_from_an_attribute()
        {
          ShouldBeTestExtensions.ShouldBe(LogSource<MockLogSource>.Category, MockLogSource.CATEGORY_NAME);
          //return valueToAnalyse;
        }
        [Test]
        public void a_log_source_without_category_is_named_after_the_type()
        {
          ShouldBeTestExtensions.ShouldBe(LogSource<LogSourceNoCategory>.Category, "LogSourceNoCategory");
          //return valueToAnalyse;
        }

        public class LogSourceNoCategory : ILogSource
        {
        }

        [LogCategory(CATEGORY_NAME)]
        class MockLogSource : ILogSource{
            public const string CATEGORY_NAME = "Mock log source";
        }
    }
}
