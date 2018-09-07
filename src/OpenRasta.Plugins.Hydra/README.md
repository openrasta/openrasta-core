## Understanding the mapping between classes and JSON

### Conventions

The following examples use an Event and a Person class.

```csharp
public class Event {
  public string Name { get; set; }
}

public class Person {
  public string Person { get; set; }
}
```

The configuration always enable the Hydra plugin, and we set the default
vocabulary for the types used in the solution.

```csharp
ResourceSpace.Uses.Hydra(options => options.Vocabulary = "https://schemas.example/schema" );
```

### Context document and scoped contexts

The Hydra plugin generates one context document per vocabulary. In the
previous example, the custom vocabulary for the current configuration is
`https://schema.example/schema`.

That schema is used at the root of the context as a `@vocab`, which
assigns each class to the correct node type. The Event class will have
an identifier of `https://schema.example/schema#Event`

Each class then defines its own vocabulary. This default model is chosen
to map to expectations developers have of the behaviour of C# classes:
two classes with the same `Name` property may not share the same definition
of what a name is.

For example, our previous Event class will always be defined in the generated
context document as follows.

```json
{
  "@context":{
    "@version": 1.1,
    "@vocab": "http://schema.example/schema#",
    "Event": {
      "@context": { "@vocab": "https://schemas.example/#Event/" }
    }
  }
}
```

This would result in the following assertions:

| Subject | Predicate | Object |
|---|---|---|
|http://my.example/Events/1|http://schema.example/schema#Event/name|Stonewall Riots|
|http://my.example/Events/1|http://www.w3.org/1999/02/22-rdf-syntax-ns#type|http://schema.example/schema#Event|

In the following sections, the context document is implicitly following
the schema above.

### Resources

A typed resource gets mapped to a json-ld resource, aka with both a `@type` and an `@id`

```chsharp

// configuration

ResourceSpace.Has
  .ResourcesOfType<Event>()
  .AtUri("/events/anExampleEvent");
  
// handler pseudocode

return new Event { Name = "Stonewall Riots" };
```

An instance of the event class wil get generated with a URI identifier as defined in the configuration.

```json
{
  "@context": "/.hydra/context.jsonld",
  "@id": "/events/anExampleEvent",
  "@type": "Event",
  "name": "Stonewall Riots"
}
```


### Blank nodes

If a resource doesn't have a URI defined in configuration, it is serialized without an `@id` property and with an `@type`.

This is rendered as a Json-Ld node object and the @type is a `node type`.

```chsharp

ResourceSpace.Has
  .ResourcesOfType<Event>()
  .Vocabulary(Vocabularies.SchemaDotOrg);
```


```json

{
  "@context": "/.hydra/context.jsonld",
  "@type": "Event",
  "name": "Stonewall Riots"
}
```

