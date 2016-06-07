using System;

namespace Is {
  public static class EqualityExtensions {
    public static void Is<T>(this T actualValue, T expectedValue) {
      if (actualValue == null && expectedValue == null) return;
      if (actualValue != null && actualValue.Equals(expectedValue)) return;
      throw new IsAssertionException($"Is not, expected \"{expectedValue}\" but had \"{actualValue}\"");
    }
  }

  public class IsAssertionException : Exception {
    public IsAssertionException(string message) : base(message) {}
  }
}