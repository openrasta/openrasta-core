using System;
using System.Linq;

namespace OpenRasta
{
  public class Error
  {
    //TODO: Consider rewriting a bit what an error is
    public string Title { get; set; }
    public string Message { get; set; }
    public Exception Exception { get; set; }

    public override string ToString()
    {
      return string.Join(Environment.NewLine,
        new[] {RenderTitle(), Message, Exception?.ToString()}.Where(x => !string.IsNullOrEmpty(x)));
    }

    public string ToStringWithoutException()
    {
      var stringWithoutException = string.Join(Environment.NewLine,
        new[] {RenderTitle(), RenderMessage()}.Where(x => !string.IsNullOrEmpty(x)).ToList());
      return stringWithoutException;
    }

    string RenderMessage() => Message ?? Exception?.Message;

    string RenderTitle() => !string.IsNullOrWhiteSpace(Title)
      ? $"{Title}:{Environment.NewLine}{new string('=', Title.Length+1)}"
      : Exception?.GetType().Name;
  }
}