using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using OpenRasta.OperationModel.MethodBased;
using OpenRasta.TypeSystem;

namespace OpenRasta.Tests.Unit.OperationModel.MethodBased
{
  public class when_there_is_a_method_filter : method_based_operation_creator_context
  {
    public Mock<IMethodFilter> MockFilter { get; set; }

    [Test]
    public void a_filter_is_called_that_filters_all_operations()
    {
      given_operation_creator(filter_selecting_first_method());
      given_handler<MockHandler>();

      when_creating_operations();

      then_operation_count_should_be(1);
      then_filter_method_was_called();
    }

    void then_filter_method_was_called()
    {
      MockFilter.VerifyAll();
    }

    IMethodFilter[] filter_selecting_first_method()
    {
      MockFilter = new Mock<IMethodFilter>();
      MockFilter.Setup(x => x.Filter(It.IsAny<IEnumerable<IMethod>>())).Returns(mock_filter()).Verifiable();
      return new[] {MockFilter.Object};
    }

    Func<IEnumerable<IMethod>, IEnumerable<IMethod>> mock_filter()
    {
      return methods => new[] {methods.First()};
    }
  }
}
