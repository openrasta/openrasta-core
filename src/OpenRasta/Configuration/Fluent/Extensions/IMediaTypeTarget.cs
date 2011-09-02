JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
﻿using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Configuration.Fluent.Extensions
{
    public interface IMediaTypeTarget : IResourceTarget
    {
        MediaTypeModel MediaType { get; }
    }
}