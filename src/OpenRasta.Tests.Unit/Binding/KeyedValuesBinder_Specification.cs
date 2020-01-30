using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Binding;
using OpenRasta.Tests.Unit.Fakes;
using OpenRasta.Tests.Unit.Infrastructure;
using OpenRasta.TypeSystem;
using OpenRasta.TypeSystem.ReflectionBased;
using Shouldly;
using TypeSystems = OpenRasta.TypeSystem.TypeSystems;

namespace KeyedValuesBinder_Specification
{
  public class when_building_the_object : context
  {
    [Test]
    public void the_object_is_created_with_the_correct_properties()
    {
      var binder = new KeyedValuesBinder(TypeOf<Customer>(), "customer");
      binder.SetProperty("username", new[] {"johndoe"}, (str, type) => BindingResult.Success(str));
      var customer = (Customer) binder.BuildObject().Instance;
      customer.Username.ShouldBe("johndoe");
    }

    [Test]
    public void the_same_object_is_returned_when_building_twice_without_changes()
    {
      var binder = new KeyedValuesBinder(TypeOf<Customer>(), "customer");

      binder.SetProperty("username", new[] {"johndoe"}, (str, type) => BindingResult.Success(str));
      var customer = (Customer) binder.BuildObject().Instance;
      var customer2 = (Customer) binder.BuildObject().Instance;

      customer.ShouldBeSameAs(customer2);
    }

    [Test]
    public void a_change_after_a_creation_results_in_a_new_oject_with_the_same_properties()
    {
      var binder = new KeyedValuesBinder(TypeOf<Customer>(), "customer");
      binder.SetProperty("username", new[] {"johndoe"}, (str, type) => BindingResult.Success(str));
      binder.BuildObject();

      binder.SetProperty("firstname", new[] {"john"}, (str, type) => BindingResult.Success(str));
      var customer = (Customer) binder.BuildObject().Instance;

      customer.Username.ShouldBe("johndoe");
      customer.FirstName.ShouldBe("john");
    }
    
    [Test]
    public void cannot_set_readonly()
    {
      var binder = new KeyedValuesBinder(TypeOf<Person>(), "aPerson");
      var canSet = binder.SetProperty("readOnlyId", new[] {"3"}, (str, type) => BindingResult.Success(str));

      canSet.ShouldBeFalse();
    }

    [Test]
    public void enumerables_are_initialized_as_empty_by_default()
    {
      var binder = new KeyedValuesBinder(TypeOf<List<Customer>>());

      var result = binder.BuildObject();
      result.Successful.ShouldBeTrue();
      result.Instance.ShouldNotBeNull();
      result.Instance.ShouldBeAssignableTo<List<Customer>>();
    }

    [Test]
    public void enumerables_can_be_built_without_headers()
    {
      var binder = new KeyedValuesBinder(TypeOf<IEnumerable<Customer>>(), "customer");
      ValueConverter<string> valueConverter = (str, type) => BindingResult.Success(type.CreateInstanceFrom(str));

      binder.SetProperty(":0.FirstName", new[] {"Frodo"}, valueConverter).ShouldBeTrue();


      binder.SetProperty(":1.FirstName", new[] {"Sam"}, valueConverter).ShouldBeTrue();

      var customers = (IEnumerable<Customer>) binder.BuildObject().Instance;
      customers.Count().ShouldBe(2);
      customers.First().FirstName.ShouldBe("Frodo");
      customers.Skip(1).First().FirstName.ShouldBe("Sam");
    }

    [Test]
    public void the_property_is_assigned_even_when_the_prefix_has_the_same_name()
    {
      var binder = new KeyedValuesBinder(TypeOf<Customer>(), "firstname");
      ValueConverter<string> valueConverter = (str, type) => BindingResult.Success(type.CreateInstanceFrom(str));

      binder.SetProperty("firstName", new[] {"Smeagol"}, valueConverter).ShouldBeTrue();

      binder.BuildObject().Instance.ShouldBeAssignableTo<Customer>()
        .FirstName.ShouldBe("Smeagol");
    }

    [Test]
    public void multiple_properties_on_child_objects_are_assigned_correctly()
    {
      var binder = new KeyedValuesBinder(TypeOf<Customer>(), "customer");
      ValueConverter<string> valueConverter = (str, type) => BindingResult.Success(type.CreateInstanceFrom(str));

      binder.SetProperty("Orders:0.Description", new[] {"something"}, valueConverter).ShouldBeTrue();


      binder.SetProperty("Orders:1.Description", new[] {"something else"}, valueConverter).ShouldBeTrue();
      binder.SetProperty("Orders:1.IsSelected", new[] {"on"}, valueConverter).ShouldBeTrue();

      binder.SetProperty("Orders:2.Description", new[] {"something"}, valueConverter).ShouldBeTrue();

      var customer = (Customer) binder.BuildObject().Instance;
      customer.Orders.Count.ShouldBe(3);
      customer.Orders[0].IsSelected.ShouldBeFalse();
      customer.Orders[1].IsSelected.ShouldBeTrue();
      customer.Orders[2].IsSelected.ShouldBeFalse();
    }

    [Test]
    public void multiple_values_with_the_same_name_for_an_icollection_appends_to_the_collection()
    {
      var binder = new KeyedValuesBinder(TypeOf<Customer>(), "firstname");
      ValueConverter<string> valueConverter = (str, type) => BindingResult.Success(type.CreateInstanceFrom(str));
      binder.SetProperty("Attributes", new[] {"blue eyes"}, valueConverter).ShouldBeTrue();
      binder.SetProperty("Attributes", new[] {"green eyes"}, valueConverter).ShouldBeTrue();

      var customer = binder.BuildObject().Instance as Customer;
      customer.Attributes.Count().ShouldBe(2);
      customer.Attributes.First().ShouldBe("blue eyes");
      customer.Attributes.Skip(1).First().ShouldBe("green eyes");
    }

    protected IType TypeOf<T>()
    {
      return TypeSystems.Default.FromClr(typeof(T));
    }
  }
}