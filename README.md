# MongoFramework
An "Entity Framework"-like interface for MongoDB

[![AppVeyor](https://img.shields.io/appveyor/ci/Turnerj/mongoframework/master.svg)](https://ci.appveyor.com/project/Turnerj/mongoframework)
[![Codecov](https://img.shields.io/codecov/c/github/turnersoftware/mongoframework/master.svg)](https://codecov.io/gh/TurnerSoftware/MongoFramework)
[![NuGet](https://img.shields.io/nuget/v/MongoFramework.svg)](https://www.nuget.org/packages/MongoFramework/)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/62fa31c90bf94f3d8e201b9684a7a4ca)](https://www.codacy.com/app/Turnerj/MongoFramework)

## Overview
MongoFramework tries to bring some of the nice features from Entity Framework into the world of MongoDB.

Some of the major features include:
- Entity mapping for collections, IDs and properties through attributes
- Indexing through attributes (including text and geospatial)
- Entity change tracking
- Changeset support (allowing for queuing multiple DB updates to run at once)
- Diff-updates (only _changes_ to an entity to be written)
- Entity mutation (allowing automatic changes on properties during select/insert/update calls)
- Navigation properties for related entities (both single-entity relationships and collections)
- Entity Buckets (clustering of small documents together, optimised for write performance)
- Runtime type discovery (serialize and deserialize without needing to specify every "known" type)

MongoFramework is currently built on-top of the official MongoDB C# driver.

## Extensions
These extensions are official packages that enhance the functionality of MongoFramework, integrating it with other systems and tools.

### MongoFramework.Profiling.MiniProfiler
[![NuGet](https://img.shields.io/nuget/v/MongoFramework.Profiling.MiniProfiler.svg)](https://www.nuget.org/packages/MongoFramework.Profiling.MiniProfiler/)

Supports profiling database reads and writes, pushing the data into [MiniProfiler](https://github.com/MiniProfiler/dotnet/).

## Documentation

### Core Entity Mapping
The core mapping of entities and their properties is automatic however there are certain attributes you can apply to your properties to alter this behaviour.
These attributes (and a few others) are part of the `System.ComponentModel.Annotations` package.

|Attribute|Description|
|---------|-----------|
|`[Table("MyFancyEntity", Schema = "MyNamespace")]`|Map the Entity to the collection specified. When a schema is specified, it is prefixed onto the name with a "." (dot) separator.|
|`[Key]`|Map the property as the "Id" for the entity. Only required if your key doesn't have a common name like "Id" etc.|
|`[NotMapped]`|Unmaps the property from the entity when reading/writing.|
|`[Column("NewColumnName")]`|Remaps the property with the specified name when reading/writing.|

### Indexing
MongoFramework supports indexing specified through the `IndexAttribute` class. This is applied to the properties you want indexed and will apply the changes to the database when the context is saved.

```csharp
public class IndexExample
{
  public string Id { get; set; }

  [Index("Email", IndexSortOrder.Ascending)]
  public string EmailAddress { get; set; }

  public string Name { get; set; }
}
```

The following variations of indexes are supported across various property types:
- [Single field](https://docs.mongodb.com/manual/core/index-single/)
- [Compound](https://docs.mongodb.com/manual/core/index-compound/#compound-indexes)
- [Multikey indexes](https://docs.mongodb.com/manual/core/index-multikey/)

To support compound indexes, define indexes with the same name across multiple properties.
When doing this, you will want to control the order of the individual items in the compound index which is available through the `IndexPriority` property on the attribute. 

#### Special Index Types
MongoFramework supports [Text](https://docs.mongodb.com/manual/core/index-text/) and [2dSphere](https://docs.mongodb.com/manual/core/2dsphere/) special indexes.
These special index types are selected through the `IndexType` property on the Index attribute.

MongoDB does have restrictions on how these indexes are used, please consult MongoDB's documentation on when the indexes are appropriate and how they are restricted.

### Contexts and Connections
Like Entity Framework, MongoFramework is built around contexts - specifically the `MongoDbContext`.
An example context would look like:

```csharp
public class MyContext : MongoDbContext
{
  public MyContext(IMongoDbConnection connection) : base(connection) { }
  public MongoDbSet<MyEntity> MyEntities { get; set; }
  public MongoDbSet<MyOtherEntity> MyOtherEntities { get; set; }
}
```

While it mostly feels the same as creating contexts in Entity Framework, there are a number of differences still with the biggest being in the creation of contexts.
The `IMongoDbConnection` is the core infrastructure that allows connection to MongoDB and is required to instantiate a context.

You can create an instance of a connection in many ways:
```csharp
IMongoDbConnection connection;

//FromUrl
connection = MongoDbConnection.FromUrl(new MongoUrl("mongodb://localhost:27017/MyDatabase")); //MongoUrl comes from the official MongoDB driver

//FromConnectionString
connection = MongoDbConnection.FromConnectionString("mongodb://localhost:27017/MyDatabase");

//FromConfig (Note: .NET Framework only)
connection = MongoDbConnection.FromConfig("MyConnectionStringName");
```

### Special Queries
You can perform text queries (against a Text index), geoNear queries (with a 2dSphere index) and geo intersecting queries.

```csharp
myContext.MyDbSet.SearchText("text to search");
myContext.MyDbSet.SearchGeoIntersecting(e => e.FieldWithCoordinates, yourGeoJsonPolygon);
myContext.MyDbSet.SearchGeoNear(e => e.FieldWithCoordinates, yourGeoJsonPoint);
```

Each of these returns an `IQueryable` which you can continue to narrow down the results.
For `SearchGeoNear` specifically, there are optional parameters for setting the distance result field, the minimum distance and the maximum distance.

### Entity Buckets
Entity buckets are a method of storing many smaller documents in fewer larger documents. MongoFramework provides various classes that help in creating and managing buckets.
A typical setup for using an entity bucket might look like:

```csharp
public class MyBucketGrouping
{
  public string ClientId { get; set; }
  public DateTime FiledDate { get; set; }
}

public class MyBucketItem
{
  public string Name { get; set; }
  public decimal Amount { get; set; }
}

public class MyContext : MongoDbContext
{
  public MyContext(IMongoDbConnection connection) : base(connection) { }
  [BucketSetOptions(BucketSize = 100)]
  public MongoDbBucketSet<MyBucketGrouping, MyBucketItem> MyBuckets { get; set; }
}
```

The attribute `BucketSetOptions` is required and `BucketSize` is the number of items in a single bucket. Keep in mind the limitations of MongoDB (size of document) when determining the number of items in a bucket.

Managing buckets is very similar to managing normal entities though are currently limited to add data only.

```csharp
using (var context = new MyContext(MongoDbConnection.FromConnectionString("mongodb://localhost:27017/MyDatabase")))
{
  context.MyBuckets.AddRange(new MyBucketGrouping
  {
    ClientId = "ABC123",
    FiledDate = DateTime.Today
  }, new []
  {
    new MyBucketItem
    {
      Name = "Foo",
      Amount = 123
    },
    new MyBucketItem
    {
      Name = "Bar",
      Amount = 456
    },
    new MyBucketItem
    {
      Name = "Baz",
      Amount = 789
    }
  });

  await context.SaveChangesAsync();
}
```

### Extra Elements
Sometimes your model in the database will have more fields than the model you are deserializing to. You have two options to control the behaviour: ignore the fields or accept, mapping the extra fields to a specific dictionary.

To ignore the fields, you need to specify the `IgnoreExtraElements` attribute on the entity's class definition.
To map the fields, you need to specify the `ExtraElements` attribute on an `IDictionary<string, object>` property.

### Property Mutation through Attribute
MongoFramework has a built-in mutation system used during reads and writes that allow you to do interesting things.

#### Built-in Attribute Mutators

|Attribute|Description|
|---------|-----------|
|`[CreatedDate]`|Populates the property with the current date/time on insert. _Note: The property must be of type `DateTime`_|
|`[UpdatedDate]`|Populates the property with the current date/time on insert. _Note: The property must be of type `DateTime`_|

#### Create Your Own

Simply extend `MongoFramework.Attributes.MutatePropertyAttribute` and overwrite the specific method when you want your mutation to fire.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class MyCustomMutatorAttribute : MutatePropertyAttribute
{
  public override void OnInsert(object target, IEntityProperty property)
  {
    //Do your mutation here! The "target" is the entity in question.
  }
}
```

### Entity Relationships
With MongoFramework's support for relationships between entities, there are specific attributes for linking an ID property to its navigation property.

`[ForeignKey("NameOfIdProperty")]` / `[ForeignKey("NameOfNavigationProperty")]`

Allows connecting two properties, an ID property and a navigation property, together in a relationship.
The ID property will be saved and updated based on the navigation property.
The navigation property won't be mapped to the entity.

`[InverseProperty("NameOfIdProperty)]`

Allows connecting an `ICollection` navigation property to a specific identifier on the related entity.
Without this attribute on a collection, the ID property of the related entity will be used.

The IDs of a one-to-many relationship are saved on the "one" as an array of IDs.
```json
{
  "_id": "5c4e495bd8ab921bf84fad1b",
  "RelatedIds": [
    "5c4e576f39530530fc4d632c",
    "5c4e576f39530530fc4d632d"
  ]
}
```

Entity relationships and navigation properties are still very new features with various limitations and quirks.
These features aren't designed to make MongoDB turn into a perfect relational database, just to make some different data access scenarios easier.

### Runtime Type Discovery
MongoFramework provides runtime type discovery in two methods: automatically for any properties of type `object` and for any entities that specify the `RuntimeTypeDiscovery` attribute on their class definition.

This type discovery means that you don't need to know what potential types extend any others which you would otherwise need to set via the `BsonKnownTypes` attribute by the MongoDB driver.

```csharp
[RuntimeTypeDiscovery]
public class KnownBaseModel
{
}

public class UnknownChildModel : KnownBaseModel
{
}

public class UnknownGrandChildModel : UnknownChildModel
{
}
```

Without the `RuntimeTypeDiscovery` attribute in this scenario, the model will fail to deserialize properly from the database.

## Complete Example
```csharp
using MongoFramework;
using System.ComponentModel.DataAnnotations;

public class MyEntity
{
  public string Id { get; set; }
  public string Name { get; set; }
  public string Description { get; set; }
}

public class MyContext : MongoDbContext
{
  public MyContext(IMongoDbConnection connection) : base(connection) { }
  public MongoDbSet<MyEntity> MyEntities { get; set; }
}

...

var connection = MongoDbConnection.FromConnectionString("YOUR_CONNECTION_STRING");
using (var myContext = new MyContext(connection))
{
  var myEntity = myContext.MyEntities.Where(myEntity => myEntity.Name == "MongoFramework").FirstOrDefault();
  myEntity.Description = "An 'Entity Framework'-like interface for MongoDB";
  await myContext.SaveChangesAsync();
}

```
