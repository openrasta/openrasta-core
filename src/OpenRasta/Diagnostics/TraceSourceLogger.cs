using System;
using System.Diagnostics;
using OpenRasta.DI;

namespace OpenRasta.Diagnostics
{
  public class TraceSourceLogger<T> : TraceSourceLogger, ILogger<T> where T : ILogSource
  {
    public TraceSourceLogger()
      : base(new TraceSource(LogSource<T>.Category))
    {
    }
  }

  public class TraceSourceLogger : ILogger
  {
    readonly TraceSource _source;
    static readonly TraceSource DefaultTraceSource = new TraceSource("openrasta");

    public static readonly ILogger Instance = new TraceSourceLogger();

    public TraceSourceLogger() : this(DefaultTraceSource)
    {
    }

    public TraceSourceLogger(TraceSource source)
    {
      _source = source;
#if CORE
      if (Debugger.IsLogging())
#else
      if (Debugger.IsLogging() && Debug.Listeners.Count == 0)
#endif
      {
        var listener = new DebuggerLoggingTraceListener
        {
          Name = "OpenRasta",
          Filter = Trace.Listeners.Count == 0
            ? new EventTypeFilter(SourceLevels.All)
            : new EventTypeFilter(SourceLevels.Verbose),
          TraceOutputOptions =
            TraceOptions.DateTime | TraceOptions.ThreadId |
            TraceOptions.LogicalOperationStack
        };

        _source.Listeners.Add(listener);
      }
      _source.Switch = new SourceSwitch("OpenRasta", "All");
    }

    public IDisposable Operation(object source, string name)
    {
      var msg = $"{source.GetType().Name} ({name})";
      _source.TraceData(TraceEventType.Start, 1, msg);
      Trace.CorrelationManager.StartLogicalOperation(source.GetType().Name);

      return new OperationCookie {Initiator = source, Source = _source, Message = msg};
    }

    public void WriteDebug(string message, params object[] format)
    {
      _source.TraceData(TraceEventType.Verbose, 0, message.With(format));
    }

    public void WriteError(string message, params object[] format)
    {
      _source.TraceData(TraceEventType.Error, 0, message.With(format));
    }

    public void WriteException(Exception e)
    {
      if (e == null)
        return;
      WriteError("An error of type {0} has been thrown", e.GetType());
      foreach (var line in e.ToString().Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries))
        WriteError(line);
    }

    public void WriteInfo(string message, params object[] format)
    {
      _source.TraceData(TraceEventType.Information, 0, message.With(format));
    }

    public void WriteWarning(string message, params object[] format)
    {
      _source.TraceData(TraceEventType.Warning, 0, message.With(format));
    }

    class OperationCookie : IDisposable
    {
      public object Initiator { get; set; }
      public TraceSource Source { get; set; }
      public string Message { get; set; }

      public void Dispose()
      {
        Source.TraceData(TraceEventType.Stop, 1, $"Exiting {Initiator.GetType().Name}: {Message}");
      }
    }
  }
}