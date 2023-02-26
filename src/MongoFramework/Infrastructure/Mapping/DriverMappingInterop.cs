using MongoDB.Bson.Serialization;

namespace MongoFramework.Infrastructure.Mapping;

/// <summary>
/// Maps the MongoFramework definition into something the MongoDB Driver will understand
/// </summary>
internal static class DriverMappingInterop
{
	/// <summary>
	/// Registers the <paramref name="definition"/> as a <see cref="BsonClassMap"/> with all appropriate properties configured.
	/// </summary>
	/// <param name="definition"></param>
	public static void RegisterDefinition(EntityDefinition definition)
	{
		var classMap = new BsonClassMap(definition.EntityType);

		// Hierarchy
		if (!EntityMapping.IsValidTypeToMap(definition.EntityType.BaseType))
		{
			classMap.SetIsRootClass(true);
		}

		// Properties
		foreach (var property in definition.Properties)
		{
			var memberMap = classMap.MapMember(property.PropertyInfo);
			memberMap.SetElementName(property.ElementName);
		}

		// Key / ID
		if (definition.Key is not null)
		{
			var idMemberMap = classMap.MapIdMember(definition.Key.Property.PropertyInfo);
			idMemberMap.SetIdGenerator(new DriverKeyGeneratorWrapper(definition.Key.KeyGenerator));
		}

		// Extra Elements
		if (definition.ExtraElements is not null)
		{
			if (definition.ExtraElements.IgnoreExtraElements)
			{
				classMap.SetIgnoreExtraElements(true);
				classMap.SetIgnoreExtraElementsIsInherited(definition.ExtraElements.IgnoreInherited);
			}
			else
			{
				classMap.SetIgnoreExtraElements(false);

				var extraElementsProperty = definition.ExtraElements.Property;
				foreach (var memberMap in classMap.DeclaredMemberMaps)
				{
					if (memberMap.ElementName == extraElementsProperty.ElementName)
					{
						classMap.SetExtraElementsMember(memberMap);
						break;
					}
				}
			}
		}

		BsonClassMap.RegisterClassMap(classMap);
	}
}
