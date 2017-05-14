using System;

namespace OpenRasta
{
    public class Error
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public override string ToString()
        {
            return $"{Title}\r\nMessage:\r\n{Message}\r\n" + Exception != null ? $"Exception:\r\n{Exception}" : string.Empty;
        }
    }
}