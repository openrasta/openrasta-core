using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using OpenRasta.Tests.Unit.Infrastructure;
using OpenRasta.TypeSystem.ReflectionBased;
using Shouldly;

namespace Extensions_Specification
{
  struct DoNothing
  {
    int value;
  }
  public class when_creating_instances_from_strings : context
  {
    [Test]
    public void empty_strings_value_type_are_deafulted()
    {
      typeof(DoNothing).CreateInstanceFrom("").ShouldBe(default(DoNothing));
    }
    [Test]
    public void empty_strings_nullables_are_defaulted()
    {
      typeof(int?).CreateInstanceFrom("")
        .ShouldBeNull();
    }
    [Test]
    public void empty_string_ints_are_defaultedd()
    {
      typeof(int).CreateInstanceFrom("")
        .ShouldBeOfType<int>().ShouldBe(0);
    }

    [Test]
    public void empty_string_bools_are_defaulted()
    {
      typeof(bool).CreateInstanceFrom("")
        .ShouldBeOfType<bool>().ShouldBe(false);
    }

    [Test]
    public void empty_string_longs_are_defaulted()
    {
      typeof(long).CreateInstanceFrom("")
        .ShouldBeOfType<long>().ShouldBe(0);
    }

    [Test]
    public void array_types_are_parsed()
    {
      typeof(int[]).CreateInstanceFrom(new[] {"1"}).ShouldBeAssignableTo<int[]>().ShouldContain(1);
    }

    [Test]
    public void arrays_of_strings_return_the_provided_values()
    {
      typeof(string[]).CreateInstanceFrom(new[] {"one", "two", "three"}).ShouldBeAssignableTo<string[]>()
        .ShouldBe((IEnumerable<string>) new[] {"one", "two", "three"});
    }

    [Test]
    public void lists_are_parsed()
    {
      typeof(List<string>).CreateInstanceFrom(new[] {"one", "two"})
        .ShouldBeAssignableTo<List<string>>()
        .ShouldBe(new[] {"one", "two"});
    }

    [Test]
    public void non_array_types_are_not_parsed_if_there_are_multiple_values()
    {
      Executing(() => typeof(int).CreateInstanceFrom(new[] {"1", "2"})).ShouldThrow<NotSupportedException>();
    }

    [Test]
    public void non_array_types_are_parsed_if_theres_one_value()
    {
      typeof(int).CreateInstanceFrom(new[] {"1"}).ShouldBeAssignableTo<int>().ShouldBe(1);
    }

    [Test]
    public void types_implementing_ICollection_of_T_are_parsed()
    {
      typeof(LinkedList<string>).CreateInstanceFrom(new[] {"one", "two"})
        .ShouldBeAssignableTo<LinkedList<string>>()
        .ShouldBe(new[] {"one", "two"});
    }
  }

  public class when_creating_type_string_for_simple_types : context
  {
    [Test]
    public void instance_results_in_typestring_for_the_instance_type()
    {
      ShouldBeTestExtensions.ShouldBe(new SimpleType().GetTypeString(), "SimpleType");
    }

    [Test]
    public void nested_types_use_the_dot_syntax()
    {
      ShouldBeTestExtensions.ShouldBe(typeof(SimpleType.NestedType).GetTypeString(), "SimpleType.NestedType");
    }

    [Test]
    public void null_types_result_in_an_error()
    {
      Executing(() => ((Type) null).GetTypeString()).ShouldThrow<ArgumentNullException>();
    }

    [Test]
    public void the_name_without_the_namespace_is_returned()
    {
      ShouldBeTestExtensions.ShouldBe(typeof(SimpleType).GetTypeString(), "SimpleType");
    }
  }

  public class when_creating_type_strings_for_generic_types : context
  {
    [Test]
    public void generic_type_strings_are_defined_with_parenthesis()
    {
      ShouldBeTestExtensions.ShouldBe(typeof(GenericType<string>).GetTypeString(), "GenericType(String)");
    }

    [Test]
    public void generic_types_that_are_not_constructed_do_not_have_a_typestring()
    {
      Executing(() => typeof(GenericType<>).GetTypeString()).ShouldThrow<InvalidOperationException>();
    }

    [Test]
    public void nested_generic_type_uses_the_generics_syntax()
    {
      ShouldBeTestExtensions.ShouldBe(typeof(SimpleType.NestedGenericType<string>).GetTypeString(),
        "SimpleType.NestedGenericType(String)");
      //return valueToAnalyse;
    }

    [Test, Ignore("Need to understand how the generics reflection api works first.")]
    public void recursive_generic_types_use_the_generic_syntax()
    {
      ShouldBeTestExtensions.ShouldBe(
        typeof(GenericType<GenericType<string>>.NestedGenericType<string>).GetTypeString(),
        "GenericType(GenericType(string)).NestedGenericType(String)");
      //return valueToAnalyse;
    }
  }

  public class when_calculating_inheritance_distances : context
  {
    [Test]
    public void a_type_implementing_an_interface_has_a_distance_of_0_to_that_interface()
    {
      typeof(IList<string>).GetInheritanceDistance(typeof(IEnumerable)).ShouldBe(0);
      //return valueToAnalyse;
    }

    [Test]
    public void an_interface_has_a_distance_of_minus_one_to_a_concrete_type()
    {
      typeof(IList).GetInheritanceDistance(typeof(string)).ShouldBe(-1);
      //return valueToAnalyse;
    }

    [Test]
    public void an_interface_has_a_distance_of_minus_one_to_an_interface_it_doesnt_implement()
    {
      typeof(IList<string>).GetInheritanceDistance(typeof(IList)).ShouldBe(-1);
      //return valueToAnalyse;
    }

    [Test]
    public void an_interface_has_a_distance_of_one_to_object()
    {
      typeof(IList).GetInheritanceDistance(typeof(object)).ShouldBe(1);
      //return valueToAnalyse;
    }

    [Test]
    public void any_type_has_a_distance_of_zero_to_itself()
    {
      typeof(int).GetInheritanceDistance(typeof(int)).ShouldBe(0);
      //return valueToAnalyse;
    }

    [Test]
    public void comparing_to_a_type_not_in_the_inheritance_tree_returns_minus_one()
    {
      typeof(int).GetInheritanceDistance(typeof(string)).ShouldBe(-1);
      //return valueToAnalyse;
    }

    [Test]
    public void primitive_types_return_one()
    {
      typeof(int).GetInheritanceDistance(typeof(ValueType)).ShouldBe(1);
      //return valueToAnalyse;
    }

    [Test]
    public void a_parent_type_has_an_inheritance_distance_of_minus_one_to_a_child_type()
    {
      typeof(ValueType).GetInheritanceDistance(typeof(int)).ShouldBe(-1);
      //return valueToAnalyse;
    }
  }

  public class when_generating_default_values : context
  {
    [Test]
    public void reference_types_return_null()
    {
      typeof(SimpleType).GetDefaultValue().ShouldBeNull();
    }

    [Test]
    public void value_types_return_a_default_instance()
    {
      typeof(int).GetDefaultValue().ShouldBe(0);
      //return valueToAnalyse;
    }
  }

  public class when_converting_to_string : context
  {
    class Converter : TypeConverter
    {
      public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture,
        object value, Type destinationType)
      {
        if (destinationType == typeof(string))
          return "ValueFromConverter";
        return null;
      }
    }

    [TypeConverter(typeof(Converter))]
    class TypeWithConverter
    {
    }

    [Test]
    public void non_primitive_types_without_converters_return_the_result_of_ToString()
    {
      var simpleType = new SimpleType();
      ShouldBeTestExtensions.ShouldBe(simpleType.ConvertToString(), simpleType.ToString());
      //return valueToAnalyse;
    }

    [Test]
    public void primitive_types_return_the_converted_value()
    {
      ShouldBeTestExtensions.ShouldBe(3.ConvertToString(), "3");
      //return valueToAnalyse;
    }

    [Test]
    public void types_with_converters_return_the_coverter_value()
    {
      ShouldBeTestExtensions.ShouldBe(new TypeWithConverter().ConvertToString(), "ValueFromConverter");
      //return valueToAnalyse;
    }
  }

  public class SimpleType
  {
    public override string ToString()
    {
      return "ToString()";
    }

    public class NestedGenericType<T>
    {
    }

    public class NestedType
    {
    }
  }

  public class GenericType<T>
  {
    public class NestedGenericType<U>
    {
    }

    public class NestedType
    {
    }
  }
}