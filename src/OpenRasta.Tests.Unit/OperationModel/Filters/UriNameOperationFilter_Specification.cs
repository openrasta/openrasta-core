using System.Linq;
using NUnit.Framework;
using OpenRasta.OperationModel.Filters;
using OpenRasta.Tests.Unit.Infrastructure;
using OpenRasta.Web;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.Filters
{
  public abstract class uriname_filter_context<THandler> : operation_filter_context<THandler, UriNameOperationFilter>
  {
    protected override UriNameOperationFilter create_filter()
    {
      return new UriNameOperationFilter(Context, UriResolver);
    }
  }

  public class uri_name_with_attributes : uriname_filter_context<Handler>
  {
    [Test]
    public void methods_with_the_attribute_are_included()
    {
      given_pipeline_selectedHandler<Handler>();
      given_uri_registration(new object(), "/", "RouteName");
      given_request_httpmethod("GET");
      given_operations();
      given_first_resource_selected();
      given_filter();

      when_filtering_operations();

      FilteredOperations.Count().ShouldBe(2);

      FilteredOperations.SingleOrDefault(x => x.Name == "GetForRouteName").ShouldNotBeNull();
      FilteredOperations.SingleOrDefault(x => x.Name == "PostForRouteName").ShouldNotBeNull();
    }
  }

  public class uri_name_with_conventions : uriname_filter_context<ConventionalHandler>
  {
    [Test]
    public void methods_matching_convention_are_included()
    {
      given_pipeline_selectedHandler<ConventionalHandler>();
      given_uri_registration(new object(), "/", "RouteName");
      given_request_httpmethod("GET");
      given_operations();
      given_first_resource_selected();

      given_filter();

      when_filtering_operations();

      FilteredOperations.Count().ShouldBe(1);

      FilteredOperations.SingleOrDefault(x => x.Name == "GetRouteName").ShouldNotBeNull();
    }
  }

  public class when_no_uri_name_is_present : uriname_filter_context<Handler>
  {
    [Test]
    public void methods_with_the_attribute_are_removed()
    {
      given_pipeline_selectedHandler<Handler>();
      given_request_httpmethod("GET");
      given_operations();
      given_uri_registration(new object(), "/");
      given_first_resource_selected();
      given_filter();

      when_filtering_operations();

      FilteredOperations.Count().ShouldBe(3);
      FilteredOperations.SingleOrDefault(x => x.Name == "GetForRouteName").ShouldBeNull();
      FilteredOperations.SingleOrDefault(x => x.Name == "PostForRouteName").ShouldBeNull();
    }
  }

  public class ConventionalHandler
  {
    public void Get()
    {
    }

    public void GetRouteName()
    {
    }
  }
}