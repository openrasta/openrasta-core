namespace Is {
    public static class EqualityExtensions {
        public static void Is<T>(this T actualValue, T expectedValue) {
            if (!actualValue.Equals(expectedValue)) {
                throw new IsAssertionException($"Is not, expected {expectedValue} but had {actualValue}");
            }
        }
    }
    public class IsAssertionException : System.Exception
    {
        public IsAssertionException( string message ) : base( message ) { }
    }
}