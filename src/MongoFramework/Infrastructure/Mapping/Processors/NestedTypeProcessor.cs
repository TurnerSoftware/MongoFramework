using MongoFramework.Infrastructure.Internal;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class NestedTypeProcessor : IMappingProcessor
	{
		public void ApplyMapping(EntityDefinitionBuilder definitionBuilder)
		{
			var entityType = definitionBuilder.EntityType;
			var properties = definitionBuilder.Properties;

			foreach (var property in properties)
			{
				var propertyType = property.PropertyInfo.PropertyType;
				propertyType = propertyType.UnwrapEnumerableTypes();

				//Maps the property type for handling property nesting
				if (propertyType != entityType && EntityMapping.IsValidTypeToMap(propertyType))
				{
					definitionBuilder.MappingBuilder.Entity(propertyType);
				}
			}
		}
	}
}
