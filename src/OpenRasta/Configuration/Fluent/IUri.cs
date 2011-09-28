namespace OpenRasta.Configuration.Fluent
{
    public interface IUri : IResource { }
    public interface IUri<TResource> : IResource<TResource>, IUri{}
    public interface IHandler : IResource { }
    public interface IHandler<TResource, THandler> : IResource<TResource> { }
}