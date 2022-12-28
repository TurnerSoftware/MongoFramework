using MongoFramework.Infrastructure.Internal;

namespace MongoFramework.Infrastructure.Mapping.Processors
{
	public class NestedTypeProcessor : IMappingProcessor
	{
		public void ApplyMapping(IEntityDefinition definition)
		{
			var entityType = definition.EntityType;
			var properties = definition.Properties;

			foreach (var property in properties)
			{
				var propertyType = property.PropertyInfo.PropertyType;
				propertyType = propertyType.GetEnumerableItemTypeOrDefault();

				//Maps the property type for handling property nesting
				if (propertyType != entityType && EntityMapping.IsValidTypeToMap(propertyType))
				{
					EntityMapping.TryRegisterType(propertyType, out _);
				}
			}
		}
	}
}
