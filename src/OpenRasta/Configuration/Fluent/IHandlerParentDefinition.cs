using System;
using OpenRasta.TypeSystem;

namespace OpenRasta.Configuration.Fluent
{
    public interface IHandlerParentDefinition : INoIzObject, IHandler
    {
        IHandlerForResourceWithUriDefinition HandledBy<T>();
        IHandlerForResourceWithUriDefinition HandledBy(Type type);
        IHandlerForResourceWithUriDefinition HandledBy(IType type);
    }
    public interface IHandlerParentDefinition<TResource> : IHandlerParentDefinition
    {
        new IHandlerForResourceWithUriDefinition<TResource, THandler> HandledBy<THandler>();
    }

    public interface IHandler { }
    public interface IHandler<TResource,THandler> { }
}