using System.Linq;
using NUnit.Framework;
using OpenRasta.DI;
using OpenRasta.Hosting;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline;
using OpenRasta.Tests.Unit.DI;
using OpenRasta.Tests.Unit.Fakes;
using Shouldly;

namespace InternalDependencyResolver_Specification
{
  public abstract class when_registering_for_per_request_lifetime : dependency_resolver_context
  {
    InMemoryContextStore InMemoryStore;

    public class TheClass
    {
    }


    public class TheClassThatNeedsYou
    {
      private readonly INeedYou _needYou;

      public TheClassThatNeedsYou(INeedYou needYou)
      {
        _needYou = needYou;
      }
    }

    public interface INeedYou
    {
    }

    public class NeedYou : INeedYou
    {
    }

    public class TheDependentClassThatNeedsYou
    {
      private readonly TheClassThatNeedsYou theClassThatNeedsYou;

      public TheDependentClassThatNeedsYou(TheClassThatNeedsYou theClassThatNeedsYou)
      {
        this.theClassThatNeedsYou = theClassThatNeedsYou;
      }
    }

    public class TheDependentClass
    {
      public TheDependentClass(TheClass dependent)
      {
        Dependent = dependent;
      }

      public TheClass Dependent { get; private set; }
    }

    void WhenClearingStore()
    {
      InMemoryStore.Clear();
    }

    void GivenInMemoryStore()
    {
      InMemoryStore = new InMemoryContextStore();
      Resolver.AddDependencyInstance<IContextStore>(InMemoryStore);
    }

    [Test]
    public void a_dependency_registered_in_one_context_is_not_registered_in_another()
    {
      var objectForScope1 = new TheClass();

      var scope1 = new AmbientContext();
      var scope2 = new AmbientContext();

      Resolver.AddDependency<IContextStore, AmbientContextStore>();

      using (new ContextScope(scope1))
      {
        Resolver.AddDependencyInstance<TheClass>(objectForScope1, DependencyLifetime.PerRequest);
      }

      using (new ContextScope(scope2))
      {
        Resolver.HasDependency(typeof(TheClass)).ShouldBeFalse();

        Executing(() => Resolver.Resolve<TheClass>()).ShouldThrow<DependencyResolutionException>();
      }
    }

    [Test]
    public void a_type_registered_as_per_request_cannot_be_resolved_if_IContextStore_is_not_registered()
    {
      Resolver.AddDependency<ISimple, Simple>(DependencyLifetime.PerRequest);

      Executing(() => Resolver.Resolve<ISimple>()).ShouldThrow<DependencyResolutionException>();
    }

    [Test]
    public void a_type_registered_as_transient_gets_an_instance_stored_in_context_injected()
    {
      Resolver.AddDependency<IContextStore, AmbientContextStore>();
      var objectForScope = new TheClass();
      var scope = new AmbientContext();
      Resolver.AddDependency<TheDependentClass>(DependencyLifetime.Transient);

      using (new ContextScope(scope))
      {
        Resolver.AddDependencyInstance(typeof(TheClass), objectForScope, DependencyLifetime.PerRequest);

        var dependentClass = Resolver.Resolve<TheDependentClass>();
        dependentClass.ShouldNotBeNull();
        dependentClass.Dependent.ShouldBeSameAs(objectForScope);
      }
    }

    [Test]
    public virtual void
      a_type_registered_as_transient_gets_an_instance_which_is_created_with_another_instance_and_is_registered_as_perwebrequest()
    {
      Resolver.AddDependency<IContextStore, AmbientContextStore>();

      Resolver.AddDependency<TheDependentClassThatNeedsYou>(DependencyLifetime.Transient);

      var contextStore = new AmbientContext();
      using (new ContextScope(contextStore))
      {
        var objectForScope = new TheClassThatNeedsYou(new NeedYou());

        Resolver.AddDependencyInstance(typeof(TheClassThatNeedsYou), objectForScope, DependencyLifetime.PerRequest);

        var dependentClass = Resolver.Resolve<TheDependentClassThatNeedsYou>();
        dependentClass.ShouldNotBeNull();
      }
    }

    [Test]
    public void non_instance_registrations_are_created_for_each_context_store()
    {
      GivenInMemoryStore();

      Resolver.AddDependency<Customer>(DependencyLifetime.PerRequest);

      var instance = Resolver.Resolve<Customer>();
      var anotherInstanceInSameContext = Resolver.Resolve<Customer>();

      instance.ShouldBeSameAs(anotherInstanceInSameContext);

      WhenClearingStore();

      var instance2 = Resolver.Resolve<Customer>();

      instance.ShouldNotBeSameAs(instance2);
    }

    [Test]
    public void registering_instances_in_different_scopes_results_in_each_consumer_getting_the_correct_registration()
    {
      var objectForScope1 = new TheClass();
      var objectForScope2 = new TheClass();
      var scope1 = new AmbientContext();
      var scope2 = new AmbientContext();

      Resolver.AddDependency<IContextStore, AmbientContextStore>();

      using (new ContextScope(scope1))
        Resolver.AddDependencyInstance<TheClass>(objectForScope1, DependencyLifetime.PerRequest);

      using (new ContextScope(scope2))
        Resolver.AddDependencyInstance<TheClass>(objectForScope2, DependencyLifetime.PerRequest);

      using (new ContextScope(scope1))
      {
        Resolver.Resolve<TheClass>().ShouldBeSameAs(objectForScope1);
      }
    }

    [Test]
    public void
      registering_instances_in_different_scopes_results_in_only_the_context_specific_registrations_to_be_resolved_in_a_context()
    {
      var objectForScope1 = new TheClass();
      var objectForScope2 = new TheClass();
      var scope1 = new AmbientContext();
      var scope2 = new AmbientContext();

      Resolver.AddDependency<IContextStore, AmbientContextStore>();

      using (new ContextScope(scope1))
        Resolver.AddDependencyInstance<TheClass>(objectForScope1, DependencyLifetime.PerRequest);

      using (new ContextScope(scope2))
        Resolver.AddDependencyInstance<TheClass>(objectForScope2, DependencyLifetime.PerRequest);

      using (new ContextScope(scope1))
      {
        Resolver.ResolveAll<TheClass>()
          .ShouldBe(new[]{objectForScope1});
      }
    }

    [Test]
    public void registering_two_instances_for_the_same_type_resolves_at_least_one_entry()
    {
      GivenInMemoryStore();
      var firstInstance = new TheClass();
      var secondInstance = new TheClass();

      Resolver.AddDependencyInstance<TheClass>(firstInstance, DependencyLifetime.PerRequest);
      Resolver.AddDependencyInstance<TheClass>(secondInstance, DependencyLifetime.PerRequest);

      var result = Resolver.ResolveAll<TheClass>();

      (result.Contains(firstInstance) || result.Contains(secondInstance)).ShouldBeTrue();
    }

    [Test]
    public void per_request_creates_new_instance_in_between_requests()
    {
      InMemoryStore = new InMemoryContextStore();
      Resolver.AddDependencyInstance<IContextStore>(InMemoryStore);

      Resolver.AddDependency<IUnknown, JohnDoe>(DependencyLifetime.PerRequest);

      var firstInstance = Resolver.Resolve<IUnknown>();


      Resolver.HandleIncomingRequestProcessed();
      InMemoryStore.Clear();
      var secondInstance = Resolver.Resolve<IUnknown>();

      firstInstance.ShouldNotBeSameAs(secondInstance);
    }

    interface IUnknown
    {
    }

    class JohnDoe : IUnknown
    {
    }

    [Test]
    public void instance_registered_as_per_request_are_cleared_when_context_store_is_terminating()
    {
      GivenInMemoryStore();
      var firstInstance = new TheClass();

      Resolver.AddDependencyInstance<TheClass>(firstInstance, DependencyLifetime.PerRequest);

      Resolver.Resolve<TheClass>().ShouldBeSameAs(firstInstance);

      Resolver.HandleIncomingRequestProcessed();

      Executing(() => Resolver.Resolve<TheClass>()).ShouldThrow<DependencyResolutionException>();
    }
  }
}