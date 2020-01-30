using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using OpenRasta.OperationModel.Filters;
using OpenRasta.Tests.Unit.Infrastructure;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.Filters
{
  namespace UriParameters_Specification
  {
    public class when_there_is_no_uri_parameter : uriparameters_context
    {
      [Test]
      public void all_operations_are_selected()
      {
        given_uri_registration(new object(), "/");
        given_first_resource_selected();
        given_operations();
        given_first_resource_selected();
        given_filter();

        when_filtering_operations();

        FilteredOperations.ShouldBe(Operations);
        Errors.Errors.Count.ShouldBe(0);
      }
    }

    public class when_there_is_one_uri_parameter_list : uriparameters_context
    {
      [Test]
      public void an_operation_having_all_parameters_is_selected()
      {
        given_operations();
        given_uri_registration(new object(), "/");
        given_first_resource_selected();
        given_pipeline_uriparams(new NameValueCollection {{"index", "42"}, {"content", "value"}});
        given_filter();

        when_filtering_operations();

        FilteredOperations.ShouldHaveSingleItem().Name.ShouldBe("Post");
        Errors.Errors.Count.ShouldBe(0);
      }

      [Test]
      public void operations_not_having_the_correct_parameter_is_excluded()
      {
        given_operations();
        given_uri_registration(new object(), "/");
        given_first_resource_selected();
        given_pipeline_uriparams(new NameValueCollection {{"unknown", "value"}, {"content", "value"}});
        given_filter();

        when_filtering_operations();

        FilteredOperations.Count().ShouldBe(0);
      }

      [Test]
      public void operations_with_the_wrong_parameter_type_are_not_selected()
      {
        given_operations();
        given_uri_registration(new object(), "/");
        given_first_resource_selected();
        given_pipeline_uriparams(new NameValueCollection {{"index", "notanumber"}, {"content", "value"}});
        given_filter();
        

        when_filtering_operations();

        FilteredOperations.Count().ShouldBe(0);
        Errors.Errors.Count.ShouldNotBe(0);
      }
      
      [Test]
      public void operations_only_selected_when_writeable_properties()
      {
        given_operations();
        given_uri_registration(new object(), "/");
        given_first_resource_selected();
        given_pipeline_uriparams(new NameValueCollection {{"id", "3"}});
        given_filter();

        when_filtering_operations();

        FilteredOperations.Count().ShouldBe(1);
      }
    }
  }

  public abstract class uriparameters_context : operation_filter_context<UriParameterFakeHandler, UriParametersFilter>
  {
    protected override UriParametersFilter create_filter()
    {
      return new UriParametersFilter(Context, Errors);
    }
  }

  public class UriParameterFakeHandler
  {
    public object Post(int index, string content)
    {
      return null;
    }

    public object PutWithId(int id) => null;
    public object PutWithType(TypeWithReadOnlyId type) => null;
  }

  public class TypeWithReadOnlyId
  {
    public int Id => 2;
  }
}