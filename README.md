# MongoFramework
An "Entity Framework"-like interface for the MongoDB C# Driver

[![AppVeyor](https://img.shields.io/appveyor/ci/Turnerj/mongoframework.svg)](https://ci.appveyor.com/project/Turnerj/mongoframework)
[![Codecov](https://img.shields.io/codecov/c/github/turnersoftware/mongoframework.svg)](https://codecov.io/gh/TurnerSoftware/MongoFramework)
[![NuGet](https://img.shields.io/nuget/v/MongoFramework.svg)](https://www.nuget.org/packages/MongoFramework/)

## Overview
MongoFramework tries to bring some of the nice features from Entity Framework into the world of MongoDB.

Some of the major features include:
- Smart entity mapping for collections, IDs and properties
- Entity change tracking
- Changeset support (allowing for queuing multiple DB updates to run at once)
- Diff-updates (only _changes_ to an entity to be written)
- Entity mutation (allowing automatic changes on properties during select/insert/update calls)
- Navigation properties for related entities (both single-entity relationships and collections)

## Entity Attributes
Through attributes on entities and their properties, you have control over various aspects of how the entities are saved or processed.

### Component Model Attributes
Following similarly to Entity Framework, MongoFramework utilises the data annotations as part of the `System.ComponentModel.Annotations` package.

`[Table("MyFancyEntity", Schema = "MyNamespace")]`

Map the Entity to the collection specified. When a schema is specified, it is prefixed onto the name with a "." (dot) separator.

`[Key]`

Map the property as the "Id" for the entity.

`[NotMapped]`

Unmaps the property from the entity when reading/writing.

`[Column("NewColumnName")]`

Remaps the property with the specified name when reading/writing.

`[ForeignKey("NameOfIdProperty")]` / `[ForeignKey("NameOfNavigationProperty")]`

Allows connecting two properties, an ID property and a navigation property, together in a relationship. The ID property will be saved and updated based on the navigation property. The navigation property won't be mapped to the entity.

`[InverseProperty("NameOfIdProperty)]`

Allows connecting an `ICollection` navigation property to a specific identifier on the related entity. Without this attribute on a collection, the ID property of the related entity will be used.

### MongoFramework-specific Attributes
There are additional attributes defined by MongoFramework that allow for more advanced functionality. These include indexing, controlling how "extra elements" are handled as well as entity mutation.

`[Index(IndexSortOrder sortOrder)]` / `[Index(string name, IndexSortOrder sortOrder)]`

Optional Parameters: `bool IsUnique`, `int IndexPriority`

Allows adding indexes to the collection. Can be defined by just the sort direction however has additional options for more advanced use.

To support [compound indexes](https://docs.mongodb.com/manual/core/index-compound/#compound-indexes), define indexes with the same name. To control the order of the indexes in the compound index, use the `IndexPriority` property (lower values are ordered first). 

`[IgnoreExtraElements]` and `[ExtraElements]`

These instruct the MongoDB driver to either ignore any extra data in the record that was fetched or to map it to a specific field. For more details, [read the documentation on the MongoDB driver](http://mongodb.github.io/mongo-csharp-driver/2.4/reference/bson/mapping/#ignoring-extra-elements)

`[CreatedDate]`

Populates the property with the current date/time on insert. _Note: The property must be of type `DateTime`_

`[UpdatedDate]`

Populates the property with the current date/time on update. _Note: The property must be of type `DateTime`_

`[IncrementNumber(int incrementAmount = 1, bool onUpdateOnly = false)]`

Updates the value of a property by the defined increment amount on insert or update.

## Example
```
using MongoFramework;
using System.ComponentModel.DataAnnotations;

public class MyEntity
{
  public string Id { get; set; }
  public string Name { get; set; }
  public string Address { get; set; }
}

public class MyContext : MongoDbContext
{
  public MyContext() : base("MyContext") { }
  public MongoDbSet<MyEntity> MyEntities { get; set; }
  public MongoDbSet<MyOtherEntity> MyOtherEntities { get; set; }
}

using (var myContext = new MyContext())
{
  var myEntity = myContext.MyEntities.Where(myEntity => myEntity.Name == "James").FirstOrDefault();
  myEntity.Address = "123 SomeAddress Road, SomeSuburb";
  myContext.SaveChanges();
}

```
