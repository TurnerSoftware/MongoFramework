# MongoFramework
An "Entity Framework"-like interface for the MongoDB C# Driver

## Key Features
- Changeset support, allowing you to queue many changes before submitting them all at once.
- Utilises .Net's data annotations to control how the entity behaves
- Object validation via data annotations on save
- Automatically detects the "Id" type and the appropriate generator for it

## Data Annotations
To allow a greater separation between the MongoDB C# Driver and your entities, MongoFramework utilises data annotations built into the .Net framework itself. Some of these control the mapping between the Entity and MongoDB while others allow for validation.

### Map-affecting Annotations
`[Table("MyFancyEntity", Schema = "MyNamespace")]`

Map the Entity to the collection specified. When a schema is specified, it is prefixed onto the name with a "." (dot) separator.

`[Key]`

Map the property as the "Id" for the entity.

`[NotMapped]`

Unmaps the property from the entity when reading/writing.

`[Column("NewColumnName")]`

Remaps the property with the specified name when reading/writing.

### Validation Annotations
Internally, MongoFramework uses the `Validator` that sits within the `System.ComponentModel.DataAnnotations` namespace allowing all built-in validation data annotations to work.

## Handling "Extra" Elements
Specify `[IgnoreExtraElements]` as an attribute on the Entity itself to allow ignoring of extra elements. If however there is a property named "ExtraElements" that is assignable from `IDictionary<object, object>`, this will automatically be populated with the "extra" elements.

## Example
```
using MongoFramework;
using System.ComponentModel.DataAnnotations;

public class MyEntity
{
  [Key]
  public string Id { get; set; }
  
  [Required]
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
  
  myContext.MyEntities.Update(myEntity);
  myContext.SaveChanges();
}

```
