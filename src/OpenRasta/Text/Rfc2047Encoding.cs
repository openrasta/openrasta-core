using System;
using System.Data.SqlTypes;
using System.Text;
using System.Globalization;
using System.IO;
using OpenRasta.Pipeline.Contributors;

namespace OpenRasta.Text
{
  /// <summary>
  /// Provides partial implementation for decoding strings according to RFC2047.
  /// </summary>
  /// <remarks>
  /// This implementation is not yet conformant to rfc2047.
  /// </remarks>
  public static class Rfc2047Encoding
  {
    class Rfc2047Decoder
    {
      int _currentPosition;
      readonly string _text;

      public Rfc2047Decoder(string text)
      {
        _text = text;
        _currentPosition = 0;
      }

      bool TryReadToken(char first, char second)
      {
        if (!IsNext(first, second)) return false;

        _currentPosition += 2;
        return true;
      }

      delegate bool TryDecode(Encoding encoder, out string output);

      bool TryGetDecoder(out TryDecode decoder)
      {
        decoder = null;
        if (TryReadToken('Q', '?') || TryReadToken('q', '?'))
        {
          decoder = TryDecodeQuoted;
          return true;
        }

        if (TryReadToken('B', '?') || TryReadToken('b', '?'))
        {
          decoder = TryDecodeBase64;
          return true;
        }

        return false;
      }

      int LengthToEndOfToken()
      {
        for (var i = _currentPosition; i < _text.Length - 1; i++)
        {
          if ((_text[i] == '?' && _text[i + 1] == '='))
            return i - _currentPosition;
        }

        return -1;
      }

      bool TryDecodeBase64(Encoding encoder, out string output)
      {
        output = null;

        var length = LengthToEndOfToken();
        if (length == -1) return false;


        var restore = _currentPosition;

        try
        {
          var bytes = Convert.FromBase64String(_text.Substring(_currentPosition, length));
          output = encoder.GetString(bytes);
          _currentPosition += length+2;
          return true;
        }
        catch
        {
          _currentPosition = restore;
          return false;
        }
      }

      int CharsLeft => _text.Length - _currentPosition;

      bool TryDecodeQuoted(Encoding encoder, out string output)
      {
        output = null;

        var originalCharacters = LengthToEndOfToken();
        
        if (originalCharacters == -1) return false;

        var binary = new byte[originalCharacters];
        var restore = _currentPosition;
        
        var parsedCharacters = originalCharacters;
        for (var i = 0; i < parsedCharacters; i++)
        {
          switch (Read())
          {
            case '_':
              binary[i] = (byte) ' ';
              break;
            case '=' when CharsLeft >= 2:
              binary[i] = byte.Parse(Read() + "" + Read(), NumberStyles.HexNumber,
                CultureInfo.InvariantCulture);
              parsedCharacters -= 2;
              break;
            case var any:
              binary[i] = (byte) any;
              break;
          }
        }

        try
        {
          output = encoder.GetString(binary, 0, parsedCharacters);
          _currentPosition += 2;
          return true;
        }
        catch
        {
          _currentPosition = restore;
          return false;
        }
      }

      public string Decode()
      {
        var sb = new StringBuilder();
        while (HasCharacters)
        {
          var pos = _currentPosition;
          if (TryReadToken('=', '?') &&
              TryParseEncoding(out var encoder) &&
              TryGetDecoder(out var tryDecode) &&
              tryDecode(encoder, out var decodedToken))
          {
            sb.Append(decodedToken);
          }
          else
          {
            _currentPosition = pos;
            sb.Append(Read());
          }
        }

        return sb.ToString();
      }

      char Current => _text[_currentPosition];

      char Read()
      {
        if (!HasCharacters) throw new InvalidOperationException("Reached the end of the string");
        _currentPosition++;
        return _text[_currentPosition - 1];
      }

      bool TryParseEncoding(out Encoding encoder)
      {
        encoder = null;

        var previousPosition = _currentPosition;

        var charsetBuilder = new StringBuilder();
        while (HasCharacters)
        {
          var ch = Read();

          if (ch == '?')
          {
            if (TryGetEncoding(charsetBuilder.ToString(), out encoder))
              return true;
            break;
          }

          charsetBuilder.Append(ch);
        }

        _currentPosition = previousPosition;
        return false;
      }

      bool TryGetEncoding(string text, out Encoding encoder)
      {
        encoder = null;
        try
        {
          encoder = Encoding.GetEncoding(text);
          return true;
        }
        catch
        {
          return false;
        }
      }

      bool IsNext(char first, char second)
        => CharsLeft >= 2 &&
           _text[_currentPosition] == first &&
           _text[_currentPosition + 1] == second;

      bool HasCharacters => _currentPosition < _text.Length;
    }

    public static string DecodeTextToken(string textToDecode)
    {
      return new Rfc2047Decoder(textToDecode).Decode();
    }
  }
}