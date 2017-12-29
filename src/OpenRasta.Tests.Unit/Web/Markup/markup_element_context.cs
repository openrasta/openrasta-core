using System;
using OpenRasta.DI;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Tests.Unit.Infrastructure;
using OpenRasta.Web;
using OpenRasta.Web.Markup;

namespace FormElement_Specification
{
  public abstract class markup_element_context<TMarkupElement> : openrasta_context
    where TMarkupElement : IElement
  {
    protected override void SetUp()
    {
      base.SetUp();
      var stub = new InMemoryCommunicationContext();
      Resolver.AddDependencyInstance(typeof(ICommunicationContext), stub);
      stub.ApplicationBaseUri = new Uri("http://localhost");
    }

    protected TMarkupElement ThenTheElement;
    protected string ThenTheElementAsString;

    protected virtual void WhenCreatingElement(Func<TMarkupElement> elementCreator)
    {
      ThenTheElement = elementCreator();
      ThenTheElementAsString = ThenTheElement.OuterXml;
    }
  }
}