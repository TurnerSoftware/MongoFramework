using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MongoFramework.Infrastructure.Mapping.Processors;

public class SkipMappingProcessor : IMappingProcessor
{
	public void ApplyMapping(EntityDefinitionBuilder definitionBuilder)
	{
		var entityType = definitionBuilder.EntityType;

		if (Attribute.IsDefined(entityType, typeof(NotMappedAttribute), true))
		{
			definitionBuilder.SkipMapping(true);
		}
	}
}
