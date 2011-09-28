JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
namespace OpenRasta.Configuration.Fluent
{
    public interface IHandlerForResourceWithUriDefinition : ICodecParentDefinition, 
                                                            IRepeatableDefinition<IHandlerParentDefinition>,
                                                            IHandler
    {
    }
    public interface IHandlerForResourceWithUriDefinition<TResource, THandler> : IHandlerForResourceWithUriDefinition, IHandler<TResource, THandler>
    {
        
    }
}