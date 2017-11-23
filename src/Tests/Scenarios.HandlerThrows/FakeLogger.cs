using System;
using System.Collections.Generic;
using OpenRasta.Diagnostics;

namespace Tests.Scenarios.HandlerThrows
{
  class FakeLogger : ILogger
  {
    readonly List<Exception> _exceptions = new List<Exception>();
    readonly List<string> _debugs = new List<string>();
    readonly List<string> _warnings = new List<string>();
    readonly List<string> _errors = new List<string>();
    readonly List<string> _infos = new List<string>();

    public IReadOnlyList<Exception> Exceptions => _exceptions;
    public IReadOnlyList<string> Debugs => _debugs;
    public IReadOnlyList<string> Warnings => _warnings;
    public IReadOnlyList<string> Errors => _errors;
    public IReadOnlyList<string> Infos => _infos;

    public IDisposable Operation(object source, string name)
    {
      return new OperationCookie();
    }

    public void WriteDebug(string message, params object[] format)
    {
      _debugs.Add(String.Format(message, format));
    }

    public void WriteWarning(string message, params object[] format)
    {
      _warnings.Add(String.Format(message, format));
    }

    public void WriteError(string message, params object[] format)
    {
      _errors.Add(String.Format(message, format));
    }

    public void WriteInfo(string message, params object[] format)
    {
      _infos.Add(String.Format(message, format));
    }

    public void WriteException(Exception e)
    {
      _exceptions.Add(e);
    }

    class OperationCookie : IDisposable
    {
      public void Dispose()
      {
      }
    }
  }
}