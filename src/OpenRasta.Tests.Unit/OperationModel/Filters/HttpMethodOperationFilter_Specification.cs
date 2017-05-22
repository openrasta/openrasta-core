using System.Linq;
using NUnit.Framework;
using OpenRasta.OperationModel.Filters;
using OpenRasta.Tests.Unit.Infrastructure;
using OpenRasta.Web;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.Filters
{
    public class when_the_http_method_matches_a_method_on_the_handler : httpmethod_context
    {
        [Test]
        public void a_method_with_a_matching_attribute_is_found()
        {

            given_pipeline_selectedHandler<Handler>();
            given_filter();
            given_request_httpmethod("PATCH");
            given_operations();

            when_filtering_operations();

          ShouldBeTestExtensions.ShouldBe(FilteredOperations.Single().Name, "ChangeData");
          //return valueToAnalyse;
        }

        [Test]
        public void the_methods_are_matched_by_name_starting_with_http_method()
        {

            given_pipeline_selectedHandler<Handler>();
            given_filter();
            given_request_httpmethod("POST");
            given_operations();

            when_filtering_operations();

          FilteredOperations.Count().ShouldBe(2);

          FilteredOperations.Count(x => x.Name == "Post").ShouldBe(1);
          //return valueToAnalyse;
          FilteredOperations.Count(x => x.Name == "PostForRouteName").ShouldBe(1);
          //return valueToAnalyse;
        }
    }

    public class httpmethod_context : operation_filter_context<Handler, HttpMethodOperationFilter>
    {
        protected override HttpMethodOperationFilter create_filter()
        {
            return new HttpMethodOperationFilter(Context.Request);
        }
    }

    public class Handler
    {
        [HttpOperation("PATCH")]
        public void ChangeData()
        {
        }

        public void Get()
        {
        }

        [HttpOperation("GET", ForUriName = "RouteName")]
        public void GetForRouteName()
        {
        }

        public void Post()
        {
        }

        [HttpOperation("POST", ForUriName = "RouteName")]
        public void PostForRouteName()
        {
        }
    }
}