using System;
using System.Buffers;
using System.Linq;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping;

public static partial class EntityMapping
{
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
			foreach (var definition in mappingBuilder.Definitions)
			{
				if (EntityDefinitions.ContainsKey(definition.EntityType))
				{
					continue;
				}

				CreateEntityDefinition(definition);
			}
		}
		finally
		{
			MappingLock.ExitWriteLock();
		}
	}

	private static EntityDefinition CreateEntityDefinition(EntityDefinitionBuilder definitionBuilder)
	{
		//TODO: Really needs a refactor - easy to see also how messy indexes make the building process
		static string GetElementName(EntityDefinitionBuilder definitionBuilder, PropertyInfo propertyInfo)
		{
			if (propertyInfo.DeclaringType == definitionBuilder.EntityType)
			{
				return definitionBuilder.Properties.First(p => p.PropertyInfo == propertyInfo).ElementName;
			}
			else if (EntityDefinitions.TryGetValue(propertyInfo.DeclaringType, out var definition))
			{
				var property = definition.GetProperty(propertyInfo.Name) ?? throw new ArgumentException($"Property \"{propertyInfo.Name}\" was not found on existing definition for \"{propertyInfo.DeclaringType}\"");
				return property.ElementName;
			}
			else
			{
				var localDefinitionBuilder = definitionBuilder.MappingBuilder.Entity(propertyInfo.DeclaringType);
				return localDefinitionBuilder.Properties.First(p => p.PropertyInfo == propertyInfo).ElementName;
			}
		}

		string EvaluateIndexPath(EntityDefinitionBuilder definitionBuilder, PropertyPath propertyPath)
		{
			var pool = ArrayPool<string>.Shared.Rent(propertyPath.Properties.Count);
			try
			{
				for (var i = 0; i < propertyPath.Properties.Count; i++)
				{
					var propertyInfo = propertyPath.Properties[i];
					pool[i] = GetElementName(definitionBuilder, propertyInfo);
				}
				return string.Join(".", pool, 0, propertyPath.Properties.Count);
			}
			finally
			{
				ArrayPool<string>.Shared.Return(pool);
			}
		}

		IndexDefinition BuildIndexDefinition(EntityDefinitionBuilder definitionBuilder, EntityIndexBuilder indexBuilder)
		{
			return new IndexDefinition
			{
				IndexPaths = indexBuilder.Properties.Select(p => new IndexPathDefinition
				{
					Path = EvaluateIndexPath(definitionBuilder, p.PropertyPath),
					IndexType = p.IndexType,
					SortOrder = p.SortOrder
				}).ToArray(),
				IndexName = indexBuilder.IndexName,
				IsUnique = indexBuilder.Unique,
				IsTenantExclusive = indexBuilder.TenantExclusive
			};
		}

		var properties = definitionBuilder.Properties.Select(p => new PropertyDefinition
		{
			PropertyInfo = p.PropertyInfo,
			ElementName = p.ElementName
		}).ToArray();

		var definition = new EntityDefinition
		{
			EntityType = definitionBuilder.EntityType,
			CollectionName = definitionBuilder.CollectionName,
			Key = definitionBuilder.KeyBuilder is null ? null : new KeyDefinition
			{
				Property = properties.First(p => p.PropertyInfo == definitionBuilder.KeyBuilder.Property),
				KeyGenerator = definitionBuilder.KeyBuilder.KeyGenerator,
			},
			Properties = properties,
			ExtraElements = definitionBuilder.ExtraElementsProperty is null ? new ExtraElementsDefinition
			{
				IgnoreExtraElements = true,
				IgnoreInherited = true
			} : new ExtraElementsDefinition
			{
				Property = properties.First(p => p.PropertyInfo == definitionBuilder.ExtraElementsProperty)
			},
			Indexes = definitionBuilder.Indexes.Select(b => BuildIndexDefinition(definitionBuilder, b)).ToArray(),
		};

		if (EntityDefinitions.TryAdd(definition.EntityType, definition))
		{
			DriverMappingInterop.RegisterDefinition(definition);
			return definition;
		}

		throw new InvalidOperationException("Uh oh");
	}
}
