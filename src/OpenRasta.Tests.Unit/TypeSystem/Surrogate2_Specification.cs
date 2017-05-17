using System;
using NUnit.Framework;
using OpenRasta.Testing;
using OpenRasta.TypeSystem;
using OpenRasta.TypeSystem.ReflectionBased;
using OpenRasta.TypeSystem.Surrogated;
using OpenRasta.TypeSystem.Surrogates;
using OpenRasta.TypeSystem.Surrogates.Static;
using Shouldly;

namespace OpenRasta.Tests.Unit.TypeSystem
{
  public class when_using_type_properties : context
  {
    readonly ITypeSystem _ts;

    public when_using_type_properties()
    {
      _ts = new ReflectionBasedTypeSystem(new SurrogateBuilderProvider(new ISurrogateBuilder[] {new Saruman()}),
        new PathManager());
    }

    [Test]
    public void can_assign_property_on_alien_type_instance()
    {
      var frodoType = _ts.FromClr<Frodo>();

      var property = frodoType.FindPropertyByPath("IsEvil");
      property.ShouldNotBeNull();

      var saruman = new Saruman();
      property.TrySetValue(saruman, false).LegacyShouldBeTrue();

      saruman.IsEvil.LegacyShouldBeFalse();
    }

    [Test]
    public void can_assign_property_on_original_type_instance()
    {
      var frodoType = _ts.FromClr<Frodo>();

      var property = frodoType.FindPropertyByPath("IsEvil");
      property.ShouldNotBeNull();

      var frodo = new Frodo();
      property.TrySetValue(frodo, true).LegacyShouldBeTrue();

      frodo.IsGood.LegacyShouldBeFalse();
    }

    [Test]
    public void owner_type_on_alien_property_is_correct()
    {
      var frodoType = _ts.FromClr<Frodo>();

      frodoType.FindPropertyByPath("IsEvil").Owner.LegacyShouldBe(frodoType);
    }

    [Test]
    public void owner_type_on_real_property_is_correct()
    {
      var frodoType = _ts.FromClr<Frodo>();

      frodoType.FindPropertyByPath("IsGood").Owner.LegacyShouldBe(frodoType);
    }

    [Test]
    public void real_properties_can_be_set()
    {
      var frodoType = _ts.FromClr<Frodo>();
      var naughty = new Frodo();
      frodoType.FindPropertyByPath("IsGood")
        .TrySetValue(naughty, false)
        .LegacyShouldBeTrue();

      naughty.IsGood.LegacyShouldBeFalse();
    }
  }

  public class when_using_type_builders : context
  {
    readonly ReflectionBasedTypeSystem _ts;

    public when_using_type_builders()
    {
      _ts = new ReflectionBasedTypeSystem(new SurrogateBuilderProvider(new ISurrogateBuilder[] {new Saruman()}),
        new PathManager());
    }

    [Test]
    public void multiple_calls_to_alien_property_executed_on_same_instance_of_surrogate()
    {
      var frodo = _ts.FromClr<Frodo>().CreateBuilder();

      var isEvilProperty = frodo.GetProperty("IsEvil");
      isEvilProperty.ShouldNotBeNull();
      bool isEvil = isEvilProperty.TrySetValue(true);

      var isMoreEvilProperty = frodo.GetProperty("IsMoreEvil");
      isMoreEvilProperty
        .ShouldNotBeNull();
      bool isMoreEvil = isMoreEvilProperty.TrySetValue(true);

      var builtFrodo = (Frodo) frodo.Create();
      builtFrodo.SarumanMessing.LegacyShouldBe(2);
    }
  }


  public class Frodo
  {
    public Frodo()
    {
      IsGood = true;
    }

    public bool IsGood { get; set; }
    public int SarumanMessing { get; set; }
  }

  public class Saruman : AbstractStaticSurrogate<Frodo>, ISurrogate
  {
    bool _isEvil = true;
    Frodo frodo;

    public bool IsEvil
    {
      get { return _isEvil; }
      set
      {
        MoodJumps++;
        if (frodo != null)
        {
          frodo.IsGood = !value;
          frodo.SarumanMessing = MoodJumps;
        }

        _isEvil = value;
      }
    }

    public bool IsMoreEvil
    {
      get { return IsEvil; }
      set { IsEvil = value; }
    }

    public int MoodJumps { get; set; }

    object ISurrogate.Value
    {
      get { return frodo; }
      set { frodo = (Frodo) value; }
    }
  }
}