using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections;

namespace OpenRasta.Plugins.Caching.Providers
{
    public class CacheKey : IEquatable<CacheKey>
    {
        string _varyAsString;
        public string Uri { get; set; }
        public IDictionary<string, string> VaryingHeaders { get; set; }

        public bool Equals(CacheKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Uri, Uri) && Equals(other._varyAsString, _varyAsString);
        }

        static string ToComparableString(IDictionary<string,string> other)
        {
            
            return other.OrderBy(_=>_.Key).Select(_=>_.Key + ": " + _.Value).JoinString(",");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(CacheKey)) return false;
            return Equals((CacheKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Uri.GetHashCode() * 397) ^ _varyAsString.GetHashCode();
            }
        }

        public CacheKey(string uri, IDictionary<string,string> varyingHeaders)
        {
            Uri = uri;
            _varyAsString = ToComparableString(varyingHeaders);
            VaryingHeaders = varyingHeaders.AsReadOnly();
        }
        public override string ToString()
        {
            return Uri + " (" + _varyAsString + ")";
        }
    }
}