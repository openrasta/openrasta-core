## Understanding the mapping between classes and JSON

The following examples use an example Event class added to an OpenRasta project.


```csharp
public class Event {
  public string Name { get; set; }
}
```

### Resources with a URI

When you associate a resource registration to a vocabulary, types wil be serialized according conventionally.
```

ResourceSpace.Has
  .ResourcesOfType<Event>()
  .Vocabulary(Vocabularies.SchemaDotOrg);
  .AtUri("/events/1");

```

An instance of the event class wil get generated with a URI identifier as defined in the configuration.

```json
{
  "@context": "/.hydra/context.jsonld",
  "@id": "/events/1",
  "@type": "Event",
  "name": "Stonewall Riots"
}
```

### Resources without a URI

If the resource doesn't have a URI defined in configuration, it is serialized without an `@id` property.

```json

{
  "@context": "/.hydra/context.jsonld",
  "@type": "Event",
  "name": "Stonewall Riots"
}
```

## Defining dynamic Hydra classes

All C# classes defined using class mapping generate a `hydra:Class` in the `ApiDocumentation`. In some situations,
classes are dynamically created from an external system. For those cases, classes can be defined separately.

```csharp
ResourceSpace.Uses
    .Hydra()
    .DynamicClasses(async ()=> {
      yield return new Hydra.Class {
        SupportedProperties = {
          new Hydra.SupportedProperty {
          }
        }
      }
    });
```