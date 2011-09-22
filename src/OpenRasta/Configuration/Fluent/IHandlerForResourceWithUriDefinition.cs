namespace OpenRasta.Configuration.Fluent
{
    public interface IHandlerForResourceWithUriDefinition : ICodecParentDefinition,
        IHandler
    {
        IHandlerParentDefinition And { get; }
    }
    public interface IHandlerForResourceWithUriDefinition<TResource, THandler> 
        : IHandlerForResourceWithUriDefinition,
          ICodecParentDefinition<TResource>,
          
          IHandler<TResource,THandler>
    {
        new IHandlerParentDefinition<TResource> And { get; }

    }
}