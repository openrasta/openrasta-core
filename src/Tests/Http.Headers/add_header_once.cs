using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Http.Headers
{
  public class content_length
  {
    HttpHeaderDictionary headers = new HttpHeaderDictionary();

    [Fact]
    public void setting_prop_to_null()
    {
      headers.ContentLength = null;
      headers.ShouldNotHaveHeader("content-length");
    }

    [Fact]
    public void setting_prop_to_length()
    {
      headers.ContentLength = 24;
      headers.ContentLength = 36;
      headers.ShouldHaveHeaderValues("content-length", "36");
    }

    [Fact]
    public void setting_header()
    {
      headers["content-length"] = "42";
      headers.ContentLength.ShouldBe(42);
    }

    [Fact]
    public void adding_header()
    {
      headers.Add("content-length", "42");
      headers.ContentLength.ShouldBe(42);
    }

    [Fact]
    public void setting_header_then_prop_to_null()
    {
      headers["content-length"] = "34";
      headers.ContentLength = null;
      headers.ShouldNotHaveHeader("content-length");
    }

    [Fact]
    public void removing_header()
    {
      headers.ContentLength = 55;
      headers.Remove("content-length");
      headers.ShouldNotHaveHeader("content-length");
      headers.ContentLength.ShouldBeNull();
    }

    [Fact]
    public void clear()
    {
      headers.ContentLength = 55;
      headers.Clear();
      headers.ShouldNotHaveHeader("content-length");
      headers.ContentLength.ShouldBeNull();
    }
  }

  public class raw_header_manipulation
  {
    HttpHeaderDictionary headers = new HttpHeaderDictionary();

    [Fact]
    public void create_from_nvc()
    {
      var nvc = new NameValueCollection();
      nvc.Add("key", "value");
      nvc.Add("key", "value2");
      headers = new HttpHeaderDictionary(nvc);
      headers.ShouldHaveHeaderValues("key", "value", "value2");
    }

    [Fact]
    public void add_once()
    {
      headers.Add("key", "value");
      headers.ShouldHaveHeaderValues("key", "value");
    }

    [Fact]
    public void add_through_kvp()
    {
      headers.Add(new KeyValuePair<string, string>("key", "value"));
      headers.ShouldHaveHeaderValues("key", "value");
    }

    [Fact]
    public void remove_through_kvp_in_multi_values()
    {
      headers.Add("key", "value0");
      headers.AddValues("key", new[] {"value1", "value2"});
      headers.Add("key", "value3");
      headers.Remove(new KeyValuePair<string, string>("key", "value2")).ShouldBeTrue();

      headers.ShouldHaveHeaderValues("key", "value0", "value1", "value3");
    }

    [Fact]
    public void remove_through_kvp_in_single_value()
    {
      headers.Add("key", "value0");
      headers.Add("key", "value1");
      headers.Remove(new KeyValuePair<string, string>("key", "value1"))
        .ShouldBeTrue();

      headers.ShouldHaveHeaderValues("key", "value0");
    }

    [Fact]
    public void remove_through_kvp_unknown_key()
    {
      headers.Remove(new KeyValuePair<string, string>("key", "value1"))
        .ShouldBeFalse();
    }

    [Fact]
    public void remove_through_kvp_unknown_value()
    {
      headers.Add("key", "value0");

      headers.Remove(new KeyValuePair<string, string>("key", "value1"))
        .ShouldBeFalse();
    }

    [Fact]
    public void add_twice()
    {
      headers.Add("key", "value1");
      headers.Add("key", "value2");
      headers.ShouldHaveHeaderValues("key", "value1", "value2");
    }

    [Fact]
    public void set()
    {
      headers["key"] = "value";
      headers.ShouldHaveHeaderValues("key", "value");
    }

    [Fact]
    public void add_then_set()
    {
      headers.Add("key", "value1");
      headers["key"] = "value2";
      headers.ShouldHaveHeaderValues("key", "value2");
    }

    [Fact]
    public void unknown_headers()
    {
      headers.ShouldNotHaveHeader("unknown");
    }

    [Fact]
    public void add_then_remove()
    {
      headers.Add("key", "value");
      headers.Add("key", "value2");
      headers.Remove("key");
      headers.ShouldNotHaveHeader("key");
    }

    [Fact]
    public void set_to_null_removes_header()
    {
      headers.Add("key", "value");
      headers["key"] = null;
      headers.ShouldNotHaveHeader("key");
    }

    [Fact]
    public void clear()
    {
      headers.Add("key", "value");
      headers.Clear();
      headers.Count.ShouldBe(0);
      headers.ShouldNotHaveHeader("key");
    }

    [Fact]
    public void many_headers()
    {
      headers.Add("key1", "value");
      headers.Add("key2", "value");
      headers.Count.ShouldBe(2);
    }

    [Fact]
    public void cannot_add_null_fields()
    {
      Should.Throw<ArgumentNullException>(() => headers.Add("key", null));
      Should.Throw<ArgumentNullException>(() => headers.Add(null, "value"));
      Should.Throw<ArgumentNullException>(() => headers.Add(null, null));
      Should.Throw<ArgumentNullException>(() => headers.ContainsKey(null));
    }
  }

  public static class HttpHeaderShouldExtensions
  {
    public static void ShouldNotHaveHeader(this HttpHeaderDictionary headers, string fieldName)
    {
      headers[fieldName].ShouldBeNull();
      headers.TryGetValue(fieldName, out _).ShouldBeFalse();

      headers.TryGetValues(fieldName, out _).ShouldBeFalse();

      headers.Keys.ShouldNotContain(fieldName);
    }

    public static void ShouldHaveHeaderValues(this HttpHeaderDictionary headers, string fieldName,
      params string[] fieldValue)
    {
      var combinedFieldValues = string.Join(",", fieldValue);

      // indexer
      headers[fieldName].ShouldBe(combinedFieldValues);

      // singular
      headers.TryGetValue(fieldName, out var singular).ShouldBeTrue();
      singular.ShouldBe(combinedFieldValues);

      // multi
      headers.TryGetValues(fieldName, out var all).ShouldBeTrue();
      all.ShouldBe(fieldValue);

      // keys contain
      headers.Keys.ShouldContain(fieldName);
      headers.ContainsKey(fieldName).ShouldBeTrue();

      // values contain (why again, why?)
      headers.Values.ShouldContain(combinedFieldValues);


      foreach (var val in fieldValue)
        headers.Contains(new KeyValuePair<string, string>(fieldName, val)).ShouldBeTrue();

      // convert to dictionary
      new Dictionary<string, string>(headers)[fieldName].ShouldBe(combinedFieldValues);

      // copy to array (why why why?)
      var array = new KeyValuePair<string, string>[10];
      headers.CopyTo(array, 0);
      array.Any(item => item.Key == fieldName && item.Value == combinedFieldValues).ShouldBeTrue();
    }
  }
}