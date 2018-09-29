using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Plugins.Hydra.Internal;
using OpenRasta.TypeSystem.ReflectionBased;
using Utf8Json;
using static System.Linq.Expressions.Expression;

namespace Tests.Plugins.Hydra.Utf8Json
{
  public static class TypeMethods
  {
    static readonly MethodInfo ResolverGetFormatterMethodInfo =
      typeof(CustomResolver).GetMethod(nameof(IJsonFormatterResolver.GetFormatter));

    static readonly PropertyInfo SerializationContextUriResolverPropertyInfo =
      typeof(SerializationContext).GetProperty(nameof(SerializationContext.UriGenerator));

    static readonly PropertyInfo SerializationContextBaseUriPropertyInfo =
      typeof(SerializationContext).GetProperty(nameof(SerializationContext.BaseUri));

    static readonly MethodInfo StringConcatTwoParamsMethodInfo =
      typeof(string).GetMethod(nameof(string.Concat), new[] {typeof(string), typeof(string)});

    public static Expression StringConcat(Expression first, Expression second)
      => Call(StringConcatTwoParamsMethodInfo, first, second);

    public static void ResourceDocument(ParameterExpression jsonWriter,
      ResourceModel model,
      Expression resource,
      Expression options,
      Action<ParameterExpression> variable,
      Action<Expression> statement, IMetaModelRepository models)
    {
      var uriResolverFunc = MakeMemberAccess(options, SerializationContextUriResolverPropertyInfo);

      var contextUri = StringConcat(
        Call(MakeMemberAccess(options, SerializationContextBaseUriPropertyInfo),
          typeof(object).GetMethod(nameof(ToString))),
        Constant(".hydra/context.jsonld"));

      foreach (var exp in WriteBeginObjectContext(jsonWriter, contextUri)) statement(exp);

      Resource(jsonWriter, model, resource, variable, statement, uriResolverFunc, models, new Stack<ResourceModel>());

      statement(JsonWriterMethods.WriteEndObject(jsonWriter));
    }

    static InvocationExpression GetUri(Expression resource, MemberExpression uriResolverFunc)
    {
      var uri = Invoke(uriResolverFunc, resource);
      return uri;
    }

    static void Resource(
      ParameterExpression jsonWriter,
      ResourceModel model,
      Expression resource,
      Action<ParameterExpression> variable,
      Action<Expression> statement,
      MemberExpression uriResolverFunc,
      IMetaModelRepository models,
      Stack<ResourceModel> recursionDefender)
    {
      if (recursionDefender.Contains(model))
        throw new InvalidOperationException(
          $"Detected recursion, already processing {model.ResourceType?.Name}: {string.Join("->", recursionDefender.Select(m => m.ResourceType?.Name).Where(n => n != null))}");

      recursionDefender.Push(model);
      var type = GetTypeName(models, model);
      foreach (var exp in model.Uris.Any()
        ? WriteIdType(jsonWriter, type, GetUri(resource, uriResolverFunc))
        : WriteType(jsonWriter, type))
        statement(exp);

      var resolver = Variable(typeof(CustomResolver), "resolver");
      variable(resolver);
      statement(Assign(resolver, New(typeof(CustomResolver))));

      foreach (var pi in model.ResourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
      {
        if (pi.CustomAttributes.Any(a => a.AttributeType.Name == "JsonIgnoreAttribute"))
          continue;

        if (pi.GetIndexParameters().Any()) continue;

        if (pi.PropertyType.IsValueType)
        {
          WriteResourceProperty(jsonWriter, variable, statement, pi, resolver,
            MakeMemberAccess(resource, pi));
          continue;
        }


        var propertyStatements = new List<Expression>();
        var propertyVars = new List<ParameterExpression>();

        // var propertyValue;
        var propertyValue = Variable(pi.PropertyType, $"val{pi.DeclaringType.Name}{pi.Name}");
        variable(propertyValue);

        // propertyValue = resource.Property;
        statement(Assign(propertyValue, MakeMemberAccess(resource, pi)));


        if (models.TryGetResourceModel(pi.PropertyType, out var propertyResourceModel))
        {
          propertyStatements.Add(JsonWriterMethods.WriteValueSeparator(jsonWriter));
          propertyStatements.Add(WritePropertyName(jsonWriter, GetJsonPropertyName(pi)));
          propertyStatements.Add(JsonWriterMethods.WriteBeginObject(jsonWriter));
          Resource(jsonWriter, propertyResourceModel, propertyValue, propertyVars.Add, propertyStatements.Add,
            uriResolverFunc, models, recursionDefender);
          propertyStatements.Add(JsonWriterMethods.WriteEndObject(jsonWriter));
        }
        else
        {
          var itemResourceRegistrations = (
            from i in pi.PropertyType.GetInterfaces()
            where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            let itemType = i.GetGenericArguments()[0]
            where itemType != typeof(object)
            let resourceModels = models.ResourceRegistrations.Where(r => itemType.IsAssignableFrom(r.ResourceType))
            where resourceModels.Any()
            orderby resourceModels.Count() descending
            select new {
              itemType, models=
                (from possible in resourceModels
              orderby possible.ResourceType.GetInheritanceDistance(itemType)
              select possible).ToList()}).FirstOrDefault();

          if (itemResourceRegistrations == null)
          {
            WriteResourceProperty(jsonWriter, propertyVars.Add, propertyStatements.Add, pi, resolver,
              MakeMemberAccess(resource, pi));
          }
          else
          {
            var itemArrayType = itemResourceRegistrations.itemType.MakeArrayType();
            var itemArray = Variable(itemArrayType);

            var toArrayMethod = typeof(Enumerable).GetMethod("ToArray")
              .MakeGenericMethod(itemResourceRegistrations.itemType);
            var assign = Assign(itemArray, Call(toArrayMethod, propertyValue));
            propertyVars.Add(itemArray);
            propertyStatements.Add(assign);

            var i = Variable(typeof(int));
            propertyVars.Add(i);

            var initialValue = Assign(i, Constant(0));
            propertyStatements.Add(initialValue);

            var itemVars = new List<ParameterExpression>();
            var itemStatements = new List<Expression>();

            var @break = Label("break");

            propertyStatements.Add(JsonWriterMethods.WriteValueSeparator(jsonWriter));
            propertyStatements.Add(WritePropertyName(jsonWriter, GetJsonPropertyName(pi)));

            propertyStatements.Add(JsonWriterMethods.WriteBeginArray(jsonWriter));

            itemStatements.Add(IfThen(GreaterThan(i, Constant(0)), JsonWriterMethods.WriteValueSeparator(jsonWriter)));
            itemStatements.Add(JsonWriterMethods.WriteBeginObject(jsonWriter));

            BlockExpression resourceBlock(ResourceModel r, ParameterExpression typed)
            {
              var vars = new List<ParameterExpression>();
              var statements = new List<Expression>();
              Resource(
                jsonWriter,
                r,
                typed,
                vars.Add, statements.Add,
                uriResolverFunc, models, recursionDefender);
              return Block(vars.ToArray(), statements.ToArray());
            }

            
            Expression renderBlock = Block(Throw(New(typeof(InvalidOperationException))));
            
            // with C : B : A, if is C else if is B else if is A else throw
            
            foreach (var specificModel in itemResourceRegistrations.models)
            {
              var typed = Variable(specificModel.ResourceType, "as" + specificModel.ResourceType.Name);
              itemVars.Add(typed);
              var @as = Assign(typed, TypeAs(ArrayAccess(itemArray, i), specificModel.ResourceType));
              renderBlock = IfThenElse(
                NotEqual(@as, Default(specificModel.ResourceType)),
                resourceBlock(specificModel, @typed),
                renderBlock);
            }

            itemStatements.Add(renderBlock);
            itemStatements.Add(PostIncrementAssign(i));
            itemStatements.Add(JsonWriterMethods.WriteEndObject(jsonWriter));
            var loop = Loop(
              IfThenElse(
                LessThan(i, MakeMemberAccess(itemArray, itemArrayType.GetProperty("Length"))),
                Block(itemVars.ToArray(), itemStatements.ToArray()),
                Break(@break)),
              @break
            );
            propertyStatements.Add(loop);
            propertyStatements.Add(JsonWriterMethods.WriteEndArray(jsonWriter));
          }
        }

        statement(IfThen(
          NotEqual(propertyValue, Default(pi.PropertyType)),
          Block(propertyVars.ToArray(), propertyStatements.ToArray())));
      }

      recursionDefender.Pop();
    }

    static string GetTypeName(IMetaModelRepository models, ResourceModel model)
    {
      var opts = models.CustomRegistrations.OfType<HydraOptions>().Single();
      var hydraResourceModel = model.Hydra();
      return (hydraResourceModel.Vocabulary?.DefaultPrefix == null
               ? string.Empty
               : $"{hydraResourceModel.Vocabulary.DefaultPrefix}:") +
             model.ResourceType.Name;
    }

    static IEnumerable<Expression> WriteIdType(
      ParameterExpression jsonWriter,
      string type,
      Expression uri)
    {
      yield return JsonWriterMethods.WriteRaw(jsonWriter, Nodes.IdProperty);

      yield return JsonWriterMethods.WriteString(jsonWriter, uri);
      yield return JsonWriterMethods.WriteValueSeparator(jsonWriter);

      yield return WritePropertyName(jsonWriter, "@type");
      yield return WriteString(jsonWriter, type);
    }

    public static IEnumerable<Expression> WriteType(ParameterExpression jsonWriter, string type)
    {
      yield return WritePropertyName(jsonWriter, "@type");
      yield return WriteString(jsonWriter, type);
    }

    static IEnumerable<Expression> WriteBeginObjectContext(ParameterExpression jsonWriter, Expression contextUri)
    {
      yield return JsonWriterMethods.WriteRaw(jsonWriter, Nodes.BeginObjectContext);
      yield return JsonWriterMethods.WriteString(jsonWriter, contextUri);
      yield return JsonWriterMethods.WriteValueSeparator(jsonWriter);
    }

    static void WriteResourceProperty(ParameterExpression jsonWriter,
      Action<ParameterExpression> variable,
      Action<Expression> statement,
      PropertyInfo pi,
      ParameterExpression jsonFormatterResolver, MemberExpression propertyGet)
    {
      statement(JsonWriterMethods.WriteValueSeparator(jsonWriter));
      var propertyName = GetJsonPropertyName(pi);


      statement(WritePropertyName(jsonWriter, propertyName));


      var propertyType = pi.PropertyType;
      var (formatterInstance, serializeMethod) = GetFormatter(variable, statement, jsonFormatterResolver, propertyType);

      var serializeFormatter =
        Call(formatterInstance, serializeMethod, jsonWriter, propertyGet, jsonFormatterResolver);
      statement(serializeFormatter);
    }

    static string GetJsonPropertyName(PropertyInfo pi)
    {
      return pi.CustomAttributes
               .Where(a => a.AttributeType.Name == "JsonPropertyAttribute")
               .SelectMany(a => a.ConstructorArguments)
               .Where(a => a.ArgumentType == typeof(string))
               .Select(a => (string) a.Value)
               .FirstOrDefault() ?? ToCamelCase(pi.Name);
    }

    static (ParameterExpression formatterInstance, MethodInfo serializeMethod) GetFormatter(
      Action<ParameterExpression> variable, Action<Expression> statement, ParameterExpression jsonFormatterResolver,
      Type propertyType)
    {
      var resolverGetFormatter = ResolverGetFormatterMethodInfo.MakeGenericMethod(propertyType);
      var jsonFormatterType = typeof(IJsonFormatter<>).MakeGenericType(propertyType);
      var serializeMethod = jsonFormatterType.GetMethod("Serialize",
        new[] {typeof(JsonWriter).MakeByRefType(), propertyType, typeof(IJsonFormatterResolver)});

      var formatterInstance = Variable(jsonFormatterType);
      statement(Assign(formatterInstance, Call(jsonFormatterResolver, resolverGetFormatter)));

      statement(IfThen(Equal(formatterInstance, Default(jsonFormatterType)),
        Throw(New(typeof(ArgumentNullException)))));
      variable(formatterInstance);
      return (formatterInstance, serializeMethod);
    }

    static string ToCamelCase(string piName)
    {
      return char.ToLowerInvariant(piName[0]) + piName.Substring(1);
    }

    public static Expression WritePropertyName(ParameterExpression jsonWriter, string propertyName)
    {
      var writer = new JsonWriter();
      writer.WritePropertyName(propertyName);
      var bytes = writer.ToUtf8ByteArray();
      return JsonWriterMethods.WriteRaw(jsonWriter, bytes);
    }

    public static Expression WriteString(ParameterExpression jsonWriter, string value)
    {
      var writer = new JsonWriter();
      writer.WriteString(value);
      var bytes = writer.ToUtf8ByteArray();
      return JsonWriterMethods.WriteRaw(jsonWriter, bytes);
    }
  }
}