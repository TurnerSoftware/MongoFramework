using System;
using System.Buffers;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping;

public static partial class EntityMapping
{
	private static readonly string PathSeparator = ".";

	public static void RegisterMapping(Action<MappingBuilder> builder)
	{
		var mappingBuilder = new MappingBuilder(MappingProcessors);
		builder(mappingBuilder);
		RegisterMapping(mappingBuilder);
	}

	public static void RegisterMapping(MappingBuilder mappingBuilder)
	{
		MappingLock.EnterWriteLock();
		try
		{
			for (var i = 0; i < mappingBuilder.Definitions.Count; i++)
			{
				var definitionBuilder = mappingBuilder.Definitions[i];
				if (EntityDefinitions.ContainsKey(definitionBuilder.EntityType))
				{
					continue;
				}

				if (definitionBuilder.MappingSkipped)
				{
					continue;
				}

				var definition = ResolveEntityDefinition(definitionBuilder);

				EntityDefinitions[definition.EntityType] = definition;
				DriverMappingInterop.RegisterDefinition(definition);
			}
		}
		finally
		{
			MappingLock.ExitWriteLock();
		}
	}

	private static KeyDefinition ResolveKeyDefinition(EntityDefinitionBuilder definitionBuilder, PropertyDefinition[] properties)
	{
		if (definitionBuilder.KeyBuilder is null)
		{
			return null;
		}

		return new KeyDefinition
		{
			Property = properties.First(p => p.PropertyInfo == definitionBuilder.KeyBuilder.Property),
			KeyGenerator = definitionBuilder.KeyBuilder.KeyGenerator,
		};
	}

	private static string ResolveElementName(EntityDefinitionBuilder definitionBuilder, PropertyInfo propertyInfo)
	{
		if (propertyInfo.DeclaringType == definitionBuilder.EntityType)
		{
			//When the definition builder is for the entity type that owns the property
			return definitionBuilder.Properties.First(p => p.PropertyInfo == propertyInfo).ElementName;
		}
		else if (EntityDefinitions.TryGetValue(propertyInfo.DeclaringType, out var definition))
		{
			//When the type that owns the property is already registered
			var property = definition.GetProperty(propertyInfo.Name) ?? throw new ArgumentException($"Property \"{propertyInfo.Name}\" was not found on existing definition for \"{propertyInfo.DeclaringType}\"");
			return property.ElementName;
		}
		else if (IsValidTypeToMap(propertyInfo.DeclaringType))
		{
			//When all else fails, find or create the appropriate definition builder for the type that owns the property
			var localDefinitionBuilder = definitionBuilder.MappingBuilder.Entity(propertyInfo.DeclaringType);
			return localDefinitionBuilder.Properties.First(p => p.PropertyInfo == propertyInfo).ElementName;
		}
		else
		{
			throw new ArgumentException($"Property \"{propertyInfo.Name}\" has a declaring type that is not valid for mapping");
		}
	}

	private static string ResolvePropertyPath(EntityDefinitionBuilder definitionBuilder, PropertyPath propertyPath)
	{
		var pool = ArrayPool<string>.Shared.Rent(propertyPath.Properties.Count);
		try
		{
			for (var i = 0; i < propertyPath.Properties.Count; i++)
			{
				var propertyInfo = propertyPath.Properties[i];
				pool[i] = ResolveElementName(definitionBuilder, propertyInfo);
			}
			return string.Join(PathSeparator, pool, 0, propertyPath.Properties.Count);
		}
		finally
		{
			ArrayPool<string>.Shared.Return(pool);
		}
	}

	private static IndexDefinition[] ResolveIndexDefinitions(EntityDefinitionBuilder definitionBuilder)
	{
		return definitionBuilder.Indexes.Select(indexBuilder => new IndexDefinition
		{
			IndexPaths = indexBuilder.Properties.Select(p => new IndexPathDefinition
			{
				Path = ResolvePropertyPath(definitionBuilder, p.PropertyPath),
				IndexType = p.IndexType,
				SortOrder = p.SortOrder
			}).ToArray(),
			IndexName = indexBuilder.IndexName,
			IsUnique = indexBuilder.Unique,
			IsTenantExclusive = indexBuilder.TenantExclusive
		}).ToArray();
	}

	private static ExtraElementsDefinition ResolveExtraElementsDefinition(EntityDefinitionBuilder definitionBuilder, PropertyDefinition[] properties)
	{
		if (definitionBuilder.ExtraElementsProperty is null)
		{
			return new ExtraElementsDefinition
			{
				IgnoreExtraElements = true,
				IgnoreInherited = true
			};
		}

		return new ExtraElementsDefinition
		{
			Property = properties.First(p => p.PropertyInfo == definitionBuilder.ExtraElementsProperty)
		};
	}

	private static EntityDefinition ResolveEntityDefinition(EntityDefinitionBuilder definitionBuilder)
	{
		var properties = definitionBuilder.Properties.Select(p => new PropertyDefinition
		{
			PropertyInfo = p.PropertyInfo,
			ElementName = p.ElementName
		}).ToArray();

		return new EntityDefinition
		{
			EntityType = definitionBuilder.EntityType,
			CollectionName = definitionBuilder.CollectionName,
			Key = ResolveKeyDefinition(definitionBuilder, properties),
			Properties = properties,
			ExtraElements = ResolveExtraElementsDefinition(definitionBuilder, properties),
			Indexes = ResolveIndexDefinitions(definitionBuilder),
		};
	}
}
