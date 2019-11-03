using System;
using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public static class SerializationContextTypes
  {
    public static readonly PropertyInfo SerializationContextUriResolverPropertyInfo =
      typeof(SerializationContext).GetProperty(nameof(SerializationContext.UriGenerator));

    public static MemberAccess<Func<object, string>> get_UriGenerator(this Variable<SerializationContext> context)
      => context.get_(c => c.UriGenerator);

    public static MemberAccess<Uri> get_BaseUri(this Variable<SerializationContext> context)
      => context.get_(_ => _.BaseUri);
    public static MemberAccess<Func<object, string>> get_TypeGenerator(this Variable<SerializationContext> context)
      => context.get_(_ => _.TypeGenerator);
  }
}