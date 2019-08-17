using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public static class SerializationContextTypes
  {
    public static readonly PropertyInfo SerializationContextUriResolverPropertyInfo =
      typeof(SerializationContext).GetProperty(nameof(SerializationContext.UriGenerator));

    public static readonly PropertyInfo SerializationContextBaseUriPropertyInfo =
      typeof(SerializationContext).GetProperty(nameof(SerializationContext.BaseUri));

    public static MemberExpression get_UriResolver(this Variable<SerializationContext> context)
      => Expression.MakeMemberAccess(context, SerializationContextTypes.SerializationContextUriResolverPropertyInfo);
    
    public static MemberExpression get_BaseUri(this Variable<SerializationContext> context)
      => Expression.MakeMemberAccess(context, SerializationContextTypes.SerializationContextBaseUriPropertyInfo);
  }
}