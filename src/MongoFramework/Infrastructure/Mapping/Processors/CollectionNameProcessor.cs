using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace MongoFramework.Infrastructure.Mapping.Processors;

public class CollectionNameProcessor : IMappingProcessor
{
	public void ApplyMapping(EntityDefinitionBuilder definitionBuilder)
	{
		var entityType = definitionBuilder.EntityType;
		var collectionName = entityType.Name;

		var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();

		if (tableAttribute == null && entityType.IsGenericType && entityType.GetGenericTypeDefinition() == typeof(EntityBucket<,>))
		{
			var groupType = entityType.GetGenericArguments()[0];
			tableAttribute = groupType.GetCustomAttribute<TableAttribute>();
			if (tableAttribute == null)
			{
				collectionName = groupType.Name;
			}
		}

		if (tableAttribute != null)
		{
			if (string.IsNullOrEmpty(tableAttribute.Schema))
			{
				collectionName = tableAttribute.Name;
			}
			else
			{
				collectionName = tableAttribute.Schema + "." + tableAttribute.Name;
			}
		}

		definitionBuilder.ToCollection(collectionName);
	}
}
