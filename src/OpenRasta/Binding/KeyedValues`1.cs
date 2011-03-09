JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
#region License
/* Authors:
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2009 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */
#endregion

using System;
using System.Collections.Generic;

namespace OpenRasta.Binding
{
    /// <summary>
    /// Represent a key associated with a series of typed values and a converter, used to match key and values pairs with binders.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    public class KeyedValues<T> : KeyedValues
    {
        public KeyedValues(string key, IEnumerable<T> values, ValueConverter<T> converter)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (values == null) throw new ArgumentNullException("values");
            Key = key;
            Values = values;
            Converter = converter;
        }


        public ValueConverter<T> Converter { get; private set; }
        public IEnumerable<T> Values { get; private set; }

        public override bool SetProperty(IObjectBinder binder)
        {
            return WasUsed = binder.SetProperty(Key, Values, Converter);
        }
    }
}

#region Full license
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion