using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class EntityIdProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition, BsonClassMap classMap)
		{
			var keyDefinition = definition.Key;
			var idProperty = keyDefinition?.Property;
			foreach (var property in definition.Properties)
			{
				if (property.PropertyInfo.GetCustomAttribute<KeyAttribute>() != null)
				{
					idProperty = property;
					break;
				}

				if (property.ElementName.Equals("id", StringComparison.InvariantCultureIgnoreCase))
				{
					//We don't break here just in case another property has the KeyAttribute
					//We preference the attribute over the name match
					idProperty = property;
				}
			}

			if (idProperty is EntityPropertyDefinition entityProperty)
			{
				var keyGenerator = keyDefinition?.KeyGenerator;

				//Set an Id Generator based on the member type
				var memberType = entityProperty.PropertyInfo.PropertyType;
				if (memberType == typeof(string))
				{
					keyGenerator = EntityKeyGenerators.StringKeyGenerator;
				}
				else if (memberType == typeof(Guid))
				{
					keyGenerator = EntityKeyGenerators.GuidKeyGenerator;
				}
				else if (memberType == typeof(ObjectId))
				{
					keyGenerator = EntityKeyGenerators.ObjectIdKeyGenerator;
				}

				definition.Key = new EntityKeyDefinition
				{
					Property = entityProperty,
					KeyGenerator = keyGenerator
				};
			}
		}
	}
}
