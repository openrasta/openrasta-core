using System.Linq;
using NUnit.Framework;
using OpenRasta.OperationModel.Filters;
using OpenRasta.Tests.Unit.Infrastructure;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.Filters
{
    public class uriname_filter_context : operation_filter_context<Handler, UriNameOperationFilter>
    {
        protected override UriNameOperationFilter create_filter()
        {
            return new UriNameOperationFilter(Context);
        }
    }

    public class when_a_uri_name_is_present : uriname_filter_context
    {
        [Test]
        public void methods_with_the_attribute_are_included()
        {
            given_pipeline_selectedHandler<Handler>();
            given_filter();
            given_request_uriName("RouteName");
            given_request_httpmethod("GET");
            given_operations();

            when_filtering_operations();

            FilteredOperations.Count().ShouldBe(2);

            FilteredOperations.SingleOrDefault(x => x.Name == "GetForRouteName").ShouldNotBeNull();
            FilteredOperations.SingleOrDefault(x => x.Name == "PostForRouteName").ShouldNotBeNull();
        }
    }
    public class when_no_uri_name_is_present : uriname_filter_context
    {
        [Test]
        public void methods_with_the_attribute_are_removed()
        {
            given_pipeline_selectedHandler<Handler>();
            given_filter();
            given_request_httpmethod("GET");
            given_operations();

            when_filtering_operations();

            FilteredOperations.Count().ShouldBe(3);
            FilteredOperations.SingleOrDefault(x => x.Name == "GetForRouteName").ShouldBeNull();
            FilteredOperations.SingleOrDefault(x => x.Name == "PostForRouteName").ShouldBeNull();
        }
    }
}
