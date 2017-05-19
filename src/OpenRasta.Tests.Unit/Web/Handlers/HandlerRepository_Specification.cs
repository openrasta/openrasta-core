using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenRasta.HandlerRepository_Specification;
using OpenRasta.Handlers;
using OpenRasta.Testing;
using OpenRasta.Tests.Unit.Fakes;
using OpenRasta.TypeSystem;
using Shouldly;

namespace HandlerRepository_Specification
{
  public class when_adding_handler_types : context
  {
    [Test]
    public void two_handlers_can_be_registered_for_the_same_key()
    {
      var handler1 = TypeSystems.Default.FromClr(typeof(Sauron));
      var handler2 = TypeSystems.Default.FromClr(typeof(Frodo));

      var repo = new HandlerRepository();

      repo.AddResourceHandler("ring of power", handler1);
      repo.AddResourceHandler("ring of power", handler2);

      repo.GetHandlerTypesFor("ring of power")
        .LegacyShouldContain(handler1)
        .LegacyShouldContain(handler2);
    }

    [Test]
    public void the_first_handler_is_returned_when_two_handlers_are_registered_for_the_same_key()
    {
      var handler1 = TypeSystems.Default.FromClr(typeof(Sauron));
      var handler2 = TypeSystems.Default.FromClr(typeof(Frodo));

      var repo = new HandlerRepository();

      repo.AddResourceHandler("ring of power", handler1);
      repo.AddResourceHandler("ring of power", handler2);

      repo.GetHandlerTypesFor("ring of power").FirstOrDefault()
        .LegacyShouldBe(handler1);
    }

    [Test]
    public void a_null_handler_cannot_be_added()
    {
      var repo = new HandlerRepository();

      Executing(() => repo.AddResourceHandler(null, TypeSystems.Default.FromClr(typeof(Frodo)))).ShouldThrow<ArgumentNullException>();
    }

    [Test]
    public void cannot_add_a_null_resource_key()
    {
      var repo = new HandlerRepository();

      Executing(() => repo.AddResourceHandler(null, TypeSystems.Default.FromClr(typeof(Frodo)))).ShouldThrow<ArgumentNullException>();
    }

    [Test]
    public void the_same_handler_can_be_registered_for_two_resources()
    {
      var gilGalad = TypeSystems.Default.FromClr(typeof(GilGalad));

      var repo = new HandlerRepository();

      repo.AddResourceHandler("Narya", gilGalad);
      repo.AddResourceHandler("Vilya", gilGalad);

      repo.GetHandlerTypesFor("Narya").FirstOrDefault().LegacyShouldBe(gilGalad);
      repo.GetHandlerTypesFor("Vilya").FirstOrDefault().LegacyShouldBe(gilGalad);
    }

    [Test]
    public void enumerating_over_the_list_of_handlers_will_only_return_distinct_handlers()
    {
      var gilGalad = TypeSystems.Default.FromClr(typeof(GilGalad));

      var repo = new HandlerRepository();

      repo.AddResourceHandler("Narya", gilGalad);
      repo.AddResourceHandler("Vilya", gilGalad);

      repo.GetHandlerTypes().LegacyShouldContain(gilGalad).Count().LegacyShouldBe(1);
    }
  }
}
