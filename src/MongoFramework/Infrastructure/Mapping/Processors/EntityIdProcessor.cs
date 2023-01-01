using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using MongoDB.Bson;

namespace MongoFramework.Infrastructure.Mapping.Processors;

public class EntityIdProcessor : IMappingProcessor
{
	public void ApplyMapping(EntityDefinitionBuilder definitionBuilder)
	{
		foreach (var propertyBuilder in definitionBuilder.Properties)
		{
			if (propertyBuilder.PropertyInfo.GetCustomAttribute<KeyAttribute>() == null)
			{
				definitionBuilder.HasKey(
					propertyBuilder.PropertyInfo,
					AutoPickKeyGenerator
				);
				return;
			}

			if (propertyBuilder.ElementName.Equals("id", StringComparison.InvariantCultureIgnoreCase))
			{
				//We don't stop here just in case another property has the KeyAttribute
				//We preference the attribute over the name match
				definitionBuilder.HasKey(
					propertyBuilder.PropertyInfo,
					AutoPickKeyGenerator
				);
			}
		}
	}

	private static void AutoPickKeyGenerator(EntityKeyBuilder keyBuilder)
	{
		var propertyType = keyBuilder.Property.PropertyType;
		if (propertyType == typeof(string))
		{
			keyBuilder.HasKeyGenerator(EntityKeyGenerators.StringKeyGenerator);
		}
		else if (propertyType == typeof(Guid))
		{
			keyBuilder.HasKeyGenerator(EntityKeyGenerators.GuidKeyGenerator);
		}
		else if (propertyType == typeof(ObjectId))
		{
			keyBuilder.HasKeyGenerator(EntityKeyGenerators.ObjectIdKeyGenerator);
		}
	}
}
