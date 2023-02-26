using System.Reflection;
using MongoFramework.Attributes;

namespace MongoFramework.Infrastructure.Mapping.Processors;

public class ExtraElementsProcessor : IMappingProcessor
{
	public void ApplyMapping(EntityDefinitionBuilder definitionBuilder)
	{
		var entityType = definitionBuilder.EntityType;

		//Ignore extra elements when the "IgnoreExtraElementsAttribute" is on the Entity
		var ignoreExtraElements = entityType.GetCustomAttribute<IgnoreExtraElementsAttribute>();
		if (ignoreExtraElements != null)
		{
			definitionBuilder.IgnoreExtraElements();
		}
		else
		{
			//If any of the Entity's properties have the "ExtraElementsAttribute", use that
			foreach (var propertyBuilder in definitionBuilder.Properties)
			{
				var extraElementsAttribute = propertyBuilder.PropertyInfo.GetCustomAttribute<ExtraElementsAttribute>();
				if (extraElementsAttribute != null)
				{
					definitionBuilder.HasExtraElements(propertyBuilder.PropertyInfo);
					break;
				}
			}
		}
	}
}
