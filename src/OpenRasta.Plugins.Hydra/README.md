## Understanding the mapping between classes and JSON-LD

### Conventions

The following examples use an Event and a Person class.

```csharp
public class Event {
  public string Name { get; set; }
  public Person Organiser { get;set; }
}

public class Person {
  public string Name { get; set; }
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

// Configuration
ResourceSpace.Has
  .ResourcesOfType<Event>()
  .AtUri("/memorable-events/stonewall-riots");
  
// Handler 
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

// Configuration
ResourceSpace.Has
  .ResourcesOfType<Event>();
  
// Handler
return new Event { Name = "Stonewall Riots" };
```


```json

{
  "@context": "/.hydra/context.jsonld",
  "@type": "Event",
  "name": "Stonewall Riots"
}
```

### Embedding

OpenRasta automatically detects the embedding of properties in one-another
and generates the links correctly.

A blank node can be embeded inside a resource.

```
// Configuration
ResourceSpace.Has
  .ResourcesOfType<Event>()
  .AtUri("/memorable-events/stonewall-riots");

// Handler
return new Event {
  Name = "Stonewall Riots",
  Organiser = new Person {
    Name = "Marsha Johnson"
  }
```

The rendered json-ld will have a resource with an embedded blank node.

```
{
  "@context": "/.hydra/context.jsonld",
  "@type": "Event",
  "@id": "/memorable-events/stonewall-riots",
  "name": "Stonewall Riots",
  "organiser": {
    "@type": "Person",
    "name": "Marsha Johnson"
  }
}
```

Resources can also embed other resources.

```
// Configuration
ResourceSpace.Has
  .ResourcesOfType<Event>()
  .AtUri("/memorable-events/stonewall-riots");
  
ResourceSpace.Has
  .ResourcesOfType<Person>()
  .AtUri("/memorable-people/pay-it-no-mind-marsha");

// Handler
return new Event {
  Name = "Stonewall Riots",
  Organiser = new Person {
    Name = "Marsha Johnson"
  }
```

The json-ld document will correctly contain identifiers for resources
and their embed resources.

```
{
  "@context": "/.hydra/context.jsonld",
  "@type": "Event",
  "@id": "/memorable-events/stonewall-riots",
  "name": "Stonewall Riots",
  "organiser": {
    "@type": "Person",
    "@id": "/memorable-people/pay-it-no-mind-marsha",
    "name": "Marsha Johnson"
  }
}
```

## Exposing Api documentation with Hydra

The Hydra Api documentation in the format specified by the hydra 
specification can be found at the URI indicated by a Link header in
HTTP responses.

```http
200 OK
Link: </.hydra/ApiDocumentation.jsonld>; rel=http://www.w3.org/ns/hydra/core#apiDocumentation
```

This document exposes the structure of the classes that are part of the
Api. It supportsall classes and all public properties on those classes.

In the events example, both the `Event` and the `Person` classes will
be documented.


```json
{
  "@context": "http://www.w3.org/ns/hydra/context.jsonld",
  "@type": "ApiDocumentation",
  "entryPoint": "/",
  "supportedClass": [
  {
    "@type": "Class",
    "@id": "https://schemas.example/schema#Event",
    "title": "Event",
    "supportedProperty": [
    {
      "@type": "SupportedProperty",
      "@id": "https://schemas.example/schema#Event/name",
      "property": {
        "@type": "rdf:Property",
        "range": "string"
      }
    }]
  },
  {
  
  }
  ]
}
```