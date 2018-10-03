using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using HandlerRepository_Specification;
using NUnit.Framework;
using OpenRasta;
using OpenRasta.Tests.Unit.Fakes;
using OpenRasta.TypeSystem;
using OpenRasta.Web;
using Shouldly;

namespace TemplatedUriResolver_Specification
{
  public class matching_uris : context.templated_uri_resolver_context
  {
    UriRegistration matching_result;

    [Test]
    public void https_uris_are_supported()
    {
      given_uri_mapping("/Valinor/Olorin", typeof(Gandalf), CultureInfo.CurrentCulture, null);
      when_matching_uri("https://localhost/Valinor/Olorin");

      matching_result
        .ShouldNotBeNull();
      matching_result.ResourceKey.ShouldBeAssignableTo<IType>()
        .Equals<Gandalf>().ShouldBeTrue();
    }

    [Test]
    public void hash_uris_are_not_matched()
    {
      given_uri_mapping("/Valinor#Olorin", typeof(Gandalf), CultureInfo.CurrentCulture, null);
      when_matching_uri("https://localhost/Valinor#Olorin");

      matching_result.ShouldBeNull();
    }

    void when_matching_uri(string uri)
    {
      matching_result = Resolver.Match(new Uri(uri));
    }
  }

  [TestFixture]
  public class creating_uris : context.templated_uri_resolver_context
  {
    [Test]
    public void a_uri_by_name_is_found()
    {
      given_uri_mapping("/location1", typeof(IConvertible), CultureInfo.CurrentCulture, null);
      given_uri_mapping("/location2", typeof(IConvertible), CultureInfo.CurrentCulture, "location2");

      when_creating_uri<IConvertible>("location2", null);

      Result.ShouldBe(new Uri("http://localhost/location2"));
    }


    [Test]
    public void the_correct_values_replace_the_variable_declarations()
    {
      given_uri_mapping("/test2/{variable2}", typeof(object), CultureInfo.CurrentCulture, null);
      given_uri_mapping("/test/{variable1}", typeof(object), CultureInfo.CurrentCulture, null);

      when_creating_uri<object>(new NameValueCollection {{"variable1", "injected1"}});

      Result.ToString().ShouldBe("http://localhost/test/injected1");
    }

    [Test]
    public void the_generated_uri_is_correct_for_generic_types()
    {
      given_uri_mapping("/test/{variable1}", typeof(IList<object>), CultureInfo.CurrentCulture, null);

      when_creating_uri<IList<object>>(new NameValueCollection {{"variable1", "injected1"}});

      Result.ToString().ShouldBe("http://localhost/test/injected1");
    }

    [Test]
    public void the_generated_uri_is_correct_when_injecting_in_query_string_variables()
    {
      given_uri_mapping("/test?query={variable1}",
        typeof(IList<object>),
        CultureInfo.CurrentCulture,
        null);

      when_creating_uri<IList<object>>(new NameValueCollection {{"variable1", "injected1"}});

      Result.ToString().ShouldBe("http://localhost/test?query=injected1");
    }

    [Test]
    public void the_generated_uri_is_correct_when_injecting_in_fragment()
    {
      given_uri_mapping("/test#before{variable1}after",
        typeof(IList<object>),
        CultureInfo.CurrentCulture,
        null);

      when_creating_uri<IList<object>>(new NameValueCollection {{"variable1", "injected1"}});

      Result.ToString().ShouldBe("http://localhost/test#beforeinjected1after");
    }
    [Test]
    public void uris_with_names_are_not_selected_by_default()
    {
      given_uri_mapping("/location1", typeof(IConvertible), CultureInfo.CurrentCulture, null);
      given_uri_mapping("/location2", typeof(IConvertible), CultureInfo.CurrentCulture, "location2");

      when_creating_uri<IConvertible>(null);

      Result.ShouldBe(new Uri("http://localhost/location1"));
    }

    [Test]
    public void uris_with_non_matching_templates_because_the_nvc_is_null_are_ignored()
    {
      given_uri_mapping("/theshire", typeof(Frodo), CultureInfo.InvariantCulture, null);
      given_uri_mapping("/theshire/{housename}", typeof(Frodo), CultureInfo.InvariantCulture, null);

      when_creating_uri<Frodo>(null);

      Result.ShouldBe(new Uri("http://localhost/theshire"));
    }

    [Test]
    public void uris_are_generated_correctly_when_base_uri_has_trailing_slash()
    {
      given_uri_mapping("/theshire", typeof(Frodo), CultureInfo.InvariantCulture, null);

      when_creating_uri<Frodo>("http://localhost/lotr/".ToUri(), null);

      Result.ShouldBe(new Uri("http://localhost/lotr/theshire"));
    }

    [Test]
    public void uris_are_generated_correctly_when_base_uri_hasnt_got_trailing_slash()
    {
      given_uri_mapping("/theshire", typeof(Frodo), CultureInfo.InvariantCulture, null);

      when_creating_uri<Frodo>("http://localhost/lotr".ToUri(), null);

      Result.ShouldBe(new Uri("http://localhost/lotr/theshire"));
    }

    [Test]
    public void uris_are_generated_correctly_for_minimum_query_fit()
    {
      given_uri_mapping("/theshire/{character}", typeof(Frodo), CultureInfo.InvariantCulture, null);
      given_uri_mapping("/theshire{character}?q={query}", typeof(Frodo), CultureInfo.InvariantCulture, null);

      when_creating_uri<Frodo>(new NameValueCollection {{"character", "frodo"}});

      Result.ShouldBe(new Uri("http://localhost/theshire/frodo"));
    }
  }

  namespace context
  {
    public class templated_uri_resolver_context : OpenRasta.Tests.Unit.Infrastructure.context
    {
      protected TemplatedUriResolver Resolver;

      protected Uri Result;
      ITypeSystem TypeSystem;

      public templated_uri_resolver_context()
      {
        TypeSystem = TypeSystems.Default;
      }

      [SetUp]
      public void before_each_behavior()
      {
        Result = null;
        Resolver = new TemplatedUriResolver();
      }

      protected void when_creating_uri<T>(Uri baseUri, NameValueCollection templateParameters)
      {
        Result = Resolver.CreateUriFor(baseUri, typeof(T), templateParameters);
      }

      protected void given_uri_mapping(string uri, Type type, CultureInfo cultureInfo, string alias)
      {
        Resolver.Add(new UriRegistration(uri, TypeSystem.FromClr(type), alias, cultureInfo));
      }

      protected void when_creating_uri<T1>(NameValueCollection nameValueCollection)
      {
        Result = Resolver.CreateUriFor(new Uri("http://localhost"), typeof(T1), nameValueCollection);
      }

      protected void when_creating_uri<T1>(string uriName, NameValueCollection nameValueCollection)
      {
        Result = Resolver.CreateUriFor(new Uri("http://localhost"), typeof(T1), uriName, nameValueCollection);
      }
    }
  }
}
