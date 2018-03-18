using OpenRasta.Configuration;
using OpenRasta.Plugins.Caching.Configuration;
using Shouldly;
using Tests.Plugins.Caching.contexts;
using Xunit;

namespace Tests.Plugins.Caching.conditionals.if_modified_since
{
  public class invalid_header_combinations : caching
  {
    /* A server that evaluates a conditional range request that is
         applicable to one of its representations MUST evaluate the condition
         as false if the entity-tag used as a validator is marked as weak or,
         when an HTTP-date is used as the validator, if the date value is not
         strong in the sense defined by Section 2.2.2 of [Part4].  (A server
         can distinguish between a valid HTTP-date and any form of entity-tag
         by examining the first two characters.)
       * The caching module can make strong guarantees on LastModifiedDate, but
       * doesn't implement range requests so we ignore If-Range and Range.
*        */
    [Theory]
    [InlineData("If-Match", "\"ETag\"")]
    [InlineData("If-Match", "W/\"ETag\"")]
    [InlineData("If-None-Match", "W/\"Etag\"")]
    [InlineData("If-None-Match", "\"Etag\"")]
    [InlineData("If-Range", "Sat, 29 Oct 1994 19:43:31 GMT")]
    [InlineData("If-Range", "W/\"Etag\"")]
    [InlineData("If-Range", "\"Etag\"")]
    public void invalid_header_combination(string header, string value)
    {
      given_current_time(now);

      given_resource<TestResource>(map => map.LastModified(_ => now - 1.Minutes()));

      given_request_header("if-modified-since", now - 2.Minutes());
      given_request_header(header, value);
      when_executing_request("/TestResource");

      response.StatusCode.ShouldBe(expected: 200);
      should_be_date(response.Headers["last-modified"], now - 1.Minutes());
    }
  }
}