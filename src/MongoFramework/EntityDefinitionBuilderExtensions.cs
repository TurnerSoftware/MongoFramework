using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework;

public static class EntityDefinitionBuilderExtensions
{
	private static PropertyInfo GetPropertyInfo(Type entityType, string propertyName)
	{
		return entityType.GetProperty(propertyName) ?? throw new ArgumentException($"Property \"{propertyName}\" can not be found on \"{entityType.Name}\".", nameof(propertyName));
	}

	public static EntityDefinitionBuilder HasKey(this EntityDefinitionBuilder definitionBuilder, string propertyName, Action<EntityKeyBuilder> builder)
		=> definitionBuilder.HasKey(GetPropertyInfo(definitionBuilder.EntityType, propertyName), builder);

	public static EntityDefinitionBuilder Ignore(this EntityDefinitionBuilder definitionBuilder, string propertyName)
		=> definitionBuilder.Ignore(GetPropertyInfo(definitionBuilder.EntityType, propertyName));

	public static EntityDefinitionBuilder HasProperty(this EntityDefinitionBuilder definitionBuilder, string propertyName, Action<EntityPropertyBuilder> builder)
		=> definitionBuilder.HasProperty(GetPropertyInfo(definitionBuilder.EntityType, propertyName), builder);

	public static EntityDefinitionBuilder HasIndex(this EntityDefinitionBuilder definitionBuilder, IEnumerable<string> propertyPaths, Action<EntityIndexBuilder> builder)
	{
		var properties = new List<IndexProperty>();
		foreach (var propertyPath in propertyPaths)
		{
			properties.Add(
				new IndexProperty(
					PropertyPath.FromString(definitionBuilder.EntityType, propertyPath)
				)
			);
		}

		return definitionBuilder.HasIndex(properties, builder);
	}
	public static EntityDefinitionBuilder HasIndex(this EntityDefinitionBuilder definitionBuilder, IEnumerable<PropertyPath> properties, Action<EntityIndexBuilder> builder)
	{
		return definitionBuilder.HasIndex(properties.Select(p => new IndexProperty(p)), builder);
	}

	public static EntityDefinitionBuilder HasExtraElements(this EntityDefinitionBuilder definitionBuilder, string propertyName)
		=> definitionBuilder.HasExtraElements(GetPropertyInfo(definitionBuilder.EntityType, propertyName));

}
