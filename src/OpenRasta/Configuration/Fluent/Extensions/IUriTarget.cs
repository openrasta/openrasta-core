JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
﻿using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Configuration.Fluent.Extensions
{
    public interface IUriTarget : IResourceTarget
    {
        UriModel Uri { get; }
    }
    public interface IHandlerTarget : IResourceTarget
    {
        HandlerModel Handler { get; }
    }
}