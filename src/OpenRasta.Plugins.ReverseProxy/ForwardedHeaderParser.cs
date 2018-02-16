using System.Collections.Generic;
using System.Text;

namespace OpenRasta.Plugins.ReverseProxy
{
  public class ForwardedHeaderParser
  {
    readonly string _value;
    StringBuilder pairKey;
    StringBuilder pairValue;
    int _pos;
    int _tokenBeginning;
    int _tokenLength;

    public ForwardedHeaderParser(string value)
    {
      _value = value;
      _pos = 0;
    }

    public IEnumerable<ForwardedHeader> Parse()
    {
      var headers = new List<ForwardedHeader>();
      while (TryReadHeader(out var header))
      {
        headers.Add(header);
        if (!TryReadChar(',')) break;
      }
      return headers;
    }

    bool TryReadHeader(out ForwardedHeader header)
    {
      header = null;
      while (TryReadPair(out var pair))
      {
        if (header == null) header = new ForwardedHeader();
        header[pair.key] = pair.value;
        if (TryPeekChar(';')) _pos++;
        else break;
      }

      return header != null;
    }

    bool TryReadPair(out (string key, string value) pair)
    {
      if (TryReadToken(out var token) && TryReadChar('=') && TryReadValue(out var value))
      {
        pair = (token, value);
        return true;
      }

      pair = default;
      return false;
    }

    bool TryReadValue(out string value)
    {
      return TryReadToken(out value) || TryReadQuotedString(out value);
    }

    bool TryReadQuotedString(out string value)
    {
      var start = _pos;
      var length = 0;
      if (!AtEnd() && TryReadChar('"'))
      {
        while (!AtEnd())
        {
          if (TryReadChar('"'))
          {
            value = length == 0 ? string.Empty : _value.Substring(start+1, length);
            return true;
          }
          _pos++;
          length++;
        }
      }

      value = null;
      _pos = start;
      return false;
    }

    bool AtEnd()
    {
      return _pos >= _value.Length;
    }

    bool TryPeekChar(char c)
    {
      if (AtEnd()) return false;
      return _value[_pos] == c;
    }
    bool TryReadChar(char c)
    {
      if (AtEnd()) return false;
      if (_value[_pos] != c) return false;

      _pos++;
      return true;
    }

    bool TryReadToken(out string token)
    {
      var start = _pos;
      var length = 0;
      while (!AtEnd() && Abnf7230Http.IsTChar(_value[_pos]))
      {
        length++;
        _pos++;
      }

      if (length == 0)
      {
        token = null;
        _pos = start;
        return false;
      }

      token = _value.Substring(start, length);
      return true;

    }
  }
}